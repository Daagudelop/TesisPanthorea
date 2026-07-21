using JuegoSerio.Datos;
using JuegoSerio.IA;
using JuegoSerio.Modelos;
using JuegoSerio.Motor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JuegoSerio.Juego
{
    // Orquesta el flujo completo del diagrama: Registro -> Termómetro -> Sesgos -> Dimensiones
    // -> Motor de Cálculo -> Reporte (ollama-unity) -> Pantalla FINAL -> (otra decisión o fin).
    //
    // La UI (botones, sliders, pantallas) NO debería tocar MotorPuntuacion/MotorPriorizacion/
    // OllamaReporteFinalGenerador directamente: solo llama a los métodos públicos de aquí y
    // escucha OnEstadoCambiado / OnReporteListo para redibujarse. Eso mantiene toda la lógica
    // de negocio en un solo lugar, testeable sin depender de escenas ni prefabs de UI.
    [RequireComponent(typeof(OllamaReporteFinalGenerador))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instancia { get; private set; }

        [SerializeField] private LocalAIResponseHandler clippy;

        [Tooltip("Opcional. Si se asigna, el facilitador conversacional puede disparar IniciarNuevaDecision() automáticamente cuando el jugador no tiene claras sus decisiones.")]
        [SerializeField] private FacilitadorConversacional facilitador;

        [Tooltip("Si está activo, el banco del Termómetro se genera con el LLM local (contextualizado a cada decisión) en vez de usar siempre banco_preguntas.json.")]
        [SerializeField] private bool generarPreguntasConIA = false;

        private List<PreguntaEje> bancoEjes; // banco fijo (banco_preguntas.json) — también sirve de respaldo
        private List<PreguntaSesgo> bancoSesgos;
        private List<PreguntaDimension> bancoDimensiones;
        private OllamaReporteFinalGenerador generadorReporte;
        private OllamaGeneradorPreguntas generadorPreguntas; // opcional; null si no está en el GameObject

        // Banco de Eficacia/Eficiencia realmente en uso para la decisión actual: el fijo, o el
        // generado por IA si 'generarPreguntasConIA' está activo. La UI siempre lee de aquí.
        private List<PreguntaEje> bancoEjesActivo;

        // Historial de la sesión completa — la "Bitácora de Decisiones".
        public List<BitacoraEntrada> Bitacora { get; } = new List<BitacoraEntrada>();

        public DecisionRegistro DecisionActual { get; private set; }
        public ResultadoDecisionCompleto ResultadoActual { get; private set; }
        public ReportePantallaFinal ReporteActual { get; private set; }
        public string CodigoTecnicoActual { get; private set; }
        public EstadoJuego Estado { get; private set; } = EstadoJuego.RegistroInicial;

        // Para que las pantallas de preguntas puedan iterar el banco activo sin acceder a los JSON.
        public IReadOnlyList<PreguntaEje> BancoEjes => bancoEjesActivo;
        public IReadOnlyList<PreguntaSesgo> BancoSesgos => bancoSesgos;
        public IReadOnlyList<PreguntaDimension> BancoDimensiones => bancoDimensiones;

        // La UI se suscribe a esto para redibujarse; ningún componente de UI conoce a los demás.
        public event Action<EstadoJuego> OnEstadoCambiado;
        public event Action<ResultadoDecisionCompleto, ReportePantallaFinal, string> OnReporteListo;

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }
            Instancia = this;
            DontDestroyOnLoad(gameObject);

            bancoEjes = CargadorBancoPreguntas.CargarBancoEjes().preguntas;
            bancoSesgos = CargadorBancoPreguntas.CargarBancoSesgos().preguntas;
            bancoDimensiones = CargadorBancoPreguntas.CargarBancoDimensiones().preguntas;
            bancoEjesActivo = bancoEjes; // por defecto, el fijo — se puede reemplazar por decisión en IniciarNuevaDecision()

            generadorReporte = GetComponent<OllamaReporteFinalGenerador>();
            generadorPreguntas = GetComponent<OllamaGeneradorPreguntas>(); // puede ser null si no se usa

            if (facilitador != null)
                facilitador.OnDecisionExtraida += decision =>
                    IniciarNuevaDecision(decision.ID_Proyecto, decision.Nombre_Decision, decision.Contexto_Problema);
        }

        // ---- Bloque 1: Registro Inicial e Identificación ----
        public async void IniciarNuevaDecision(string idProyecto, string nombreDecision, string contextoProblema)
        {
            DecisionActual = new DecisionRegistro
            {
                ID_Proyecto = idProyecto,
                Nombre_Decision = nombreDecision,
                Contexto_Problema = contextoProblema
            };
            ResultadoActual = null;
            ReporteActual = null;
            CodigoTecnicoActual = null;

            if (generarPreguntasConIA && generadorPreguntas != null)
            {
                CambiarEstado(EstadoJuego.GenerandoPreguntas);
                // Si la generación falla, GenerarPreguntasEjeAsync() ya devuelve 'bancoEjes'
                // (el banco fijo) como respaldo — nunca nos quedamos sin preguntas.
                bancoEjesActivo = await generadorPreguntas.GenerarPreguntasEjeAsync(DecisionActual, bancoEjes);
            }
            else
            {
                bancoEjesActivo = bancoEjes;
            }

            CambiarEstado(EstadoJuego.ResponderTermometro);
        }

        // ---- Registrar respuestas (la UI llama a esto pregunta por pregunta) ----
        public void RegistrarRespuestaEje(Respuesta respuesta) => DecisionActual?.RespuestasEjes.Add(respuesta);
        public void RegistrarRespuestaSesgo(Respuesta respuesta) => DecisionActual?.RespuestasSesgos.Add(respuesta);
        public void RegistrarRespuestaDimension(Respuesta respuesta) => DecisionActual?.RespuestasDimensiones.Add(respuesta);

        // ---- Transiciones entre bloques de preguntas ----
        public void TerminarTermometro() => CambiarEstado(EstadoJuego.ResponderSesgos);
        public void TerminarSesgos() => CambiarEstado(EstadoJuego.ResponderDimensiones);

        // Cierra el registro de preguntas, corre el motor de cálculo y pide el texto al LLM local.
        public async void TerminarDimensionesYCalcular()
        {
            if (DecisionActual == null)
            {
                Debug.LogError("[JuegoSerio] No hay una decisión activa. Llama a IniciarNuevaDecision() primero.");
                return;
            }

            CambiarEstado(EstadoJuego.Calculando);

            ResultadoActual = OrquestadorMotorDecision.Evaluar(DecisionActual, bancoEjesActivo, bancoSesgos, bancoDimensiones);
            CodigoTecnicoActual = CodigoTecnicoGenerador.Generar(ResultadoActual);
            ReporteActual = await generadorReporte.GenerarAsync(DecisionActual, ResultadoActual);

            Bitacora.Add(new BitacoraEntrada
            {
                Decision = DecisionActual,
                Resultado = ResultadoActual,
                Reporte = ReporteActual,
                CodigoTecnico = CodigoTecnicoActual
            });

            OnReporteListo?.Invoke(ResultadoActual, ReporteActual, CodigoTecnicoActual);
            CambiarEstado(EstadoJuego.PantallaFinal);

            // Refleja en Clippy el resultado REAL del Termómetro (ya no simulado por teclado)
            EnviarRespuestaClippy(ResultadoActual.Termometro.EsEficaz, ResultadoActual.Termometro.EsEficiente);
        }

        private void EnviarRespuestaClippy(bool esEficaz, bool esEficiente)
        {
            string json = $"{{\"esEficaz\":{(esEficaz ? "true" : "false")},\"esEficiente\":{(esEficiente ? "true" : "false")}}}";
            clippy.SendTestResponse(json);
        }

        // ---- Bloque "¿Ha registrado todas las decisiones?" (llamado desde la Pantalla FINAL) ----
        public void RegistrarOtraDecision() => CambiarEstado(EstadoJuego.RegistroInicial);
        public void TerminarSesion() => CambiarEstado(EstadoJuego.FinDeSesion);

        private void CambiarEstado(EstadoJuego nuevo)
        {
            Estado = nuevo;
            OnEstadoCambiado?.Invoke(Estado);
        }

    }
}

using UnityEngine;
using JuegoSerio.Modelos;
using JuegoSerio.Juego;

namespace JuegoSerio.UI
{
    // Único script que conoce a todas las pantallas a la vez: activa/desactiva el GameObject
    // raíz de cada panel según GameManager.Estado, y le pasa el resultado listo a PantallaFinalUI.
    // Ninguna pantalla individual necesita saber que las demás existen.
    public class ControladorPantallas : MonoBehaviour
    {
        [Header("Paneles (GameObjects raíz de cada pantalla)")]
        [SerializeField] private GameObject panelRegistro;   // el de PantallaRegistroUI o PantallaChatFacilitadorUI, el que uses
        [SerializeField] private GameObject panelPreguntas;  // reutilizado en Termometro/Sesgos/Dimensiones
        [SerializeField] private GameObject panelCalculando;
        [SerializeField] private GameObject panelFinal;
        [SerializeField] private GameObject panelFinDeSesion;

        [Header("Referencias a los scripts de cada pantalla")]
        [SerializeField] private PantallaPreguntasUI pantallaPreguntas;
        [SerializeField] private PantallaFinalUI pantallaFinal;

        private void Start()
        {
            // Start() (a diferencia de OnEnable()) está garantizado por Unity a correr DESPUÉS
            // de que todos los Awake() de la escena ya se ejecutaron — incluido el de
            // GameManager, que es donde se asigna GameManager.Instancia. Suscribirse en
            // OnEnable() podía fallar si este objeto se activaba antes que el GameManager.
            if (GameManager.Instancia == null)
            {
                Debug.LogError("[JuegoSerio] No hay un GameManager en la escena. Agrega un GameObject con el componente GameManager.");
                return;
            }

            GameManager.Instancia.OnEstadoCambiado += ManejarCambioEstado;
            GameManager.Instancia.OnReporteListo += ManejarReporteListo;
            ManejarCambioEstado(GameManager.Instancia.Estado); // sincroniza el panel visible al arrancar
        }

        private void OnDisable()
        {
            if (GameManager.Instancia == null) return;
            GameManager.Instancia.OnEstadoCambiado -= ManejarCambioEstado;
            GameManager.Instancia.OnReporteListo -= ManejarReporteListo;
        }

        private void ManejarCambioEstado(EstadoJuego estado)
        {
            panelRegistro.SetActive(estado == EstadoJuego.RegistroInicial);

            bool enBloqueDePreguntas = estado == EstadoJuego.ResponderTermometro
                                     || estado == EstadoJuego.ResponderSesgos
                                     || estado == EstadoJuego.ResponderDimensiones;
            panelPreguntas.SetActive(enBloqueDePreguntas);

            panelCalculando.SetActive(estado == EstadoJuego.Calculando || estado == EstadoJuego.GenerandoPreguntas);
            panelFinal.SetActive(estado == EstadoJuego.PantallaFinal);
            panelFinDeSesion.SetActive(estado == EstadoJuego.FinDeSesion);

            switch (estado)
            {
                case EstadoJuego.ResponderTermometro: pantallaPreguntas.MostrarBloque(BloquePreguntas.Termometro); break;
                case EstadoJuego.ResponderSesgos: pantallaPreguntas.MostrarBloque(BloquePreguntas.Sesgos); break;
                case EstadoJuego.ResponderDimensiones: pantallaPreguntas.MostrarBloque(BloquePreguntas.Dimensiones); break;
            }
        }

        private void ManejarReporteListo(ResultadoDecisionCompleto resultado, ReportePantallaFinal reporte, string codigoTecnico) =>
            pantallaFinal.MostrarResultado(resultado, reporte, codigoTecnico);
    }
}

using UnityEngine;
using JuegoSerio.Modelos;
using JuegoSerio.Juego;

namespace JuegoSerio.Demo
{
    // Simula lo que haría la UI real: se suscribe a los eventos del GameManager y dispara
    // los métodos públicos en orden, sin ninguna pantalla de por medio. Útil para probar el
    // flujo completo antes de tener el UI Toolkit / Canvas armado.
    // Requiere un GameObject con GameManager + OllamaReporteFinalGenerador en la escena.
    public class PruebaFlujoGameManager : MonoBehaviour
    {
        private void Start()
        {
            GameManager.Instancia.OnEstadoCambiado += estado => Debug.Log($"[Estado] -> {estado}");
            GameManager.Instancia.OnReporteListo += (resultado, reporte, codigo) =>
            {
                Debug.Log($"Código Técnico: {codigo}");
                Debug.Log($"Resultado final: {reporte.ResultadoFinal}");
                Debug.Log($"CTA: {reporte.CTA}");
            };

            SimularDecisionCompleta();
        }

        private void SimularDecisionCompleta()
        {
            var gm = GameManager.Instancia;

            // Bloque 1: Registro Inicial
            gm.IniciarNuevaDecision(
                "PROY-01",
                "Cambiar de motor de renderizado a mitad de producción",
                "El equipo detectó cuellos de botella de rendimiento en la build vertical.");

            // Bloque 2: Termómetro — la UI recorrería gm.BancoEjes mostrando una pregunta a la vez
            gm.RegistrarRespuestaEje(Respuesta.CrearEscala("INT_001", 3));
            gm.RegistrarRespuestaEje(Respuesta.CrearSiNo("INT_002", false));
            gm.RegistrarRespuestaEje(Respuesta.CrearEscala("INT_003", 2));
            gm.RegistrarRespuestaEje(Respuesta.CrearSiNo("INT_004", true));
            gm.RegistrarRespuestaEje(Respuesta.CrearEscala("ADA_001", 4));
            gm.RegistrarRespuestaEje(Respuesta.CrearEscala("ADA_003", 5));
            gm.RegistrarRespuestaEje(Respuesta.CrearSiNo("ADA_006", true));
            gm.TerminarTermometro();

            // Bloque 4 (preguntas): Sesgos — la UI recorrería gm.BancoSesgos
            gm.RegistrarRespuestaSesgo(Respuesta.CrearSiNo("SESG_004", true));
            gm.RegistrarRespuestaSesgo(Respuesta.CrearEscala("SESG_005", 4));
            gm.TerminarSesgos();

            // Diccionario_Dimensiones — la UI recorrería gm.BancoDimensiones
            gm.RegistrarRespuestaDimension(Respuesta.CrearSiNo("EST-01", true));
            gm.RegistrarRespuestaDimension(Respuesta.CrearSiNo("EST-02", true));
            gm.RegistrarRespuestaDimension(Respuesta.CrearSiNo("OPE-03", true));

            // Motor de cálculo + LLM local -> Pantalla FINAL
            gm.TerminarDimensionesYCalcular();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using JuegoSerio.Modelos;
using JuegoSerio.Motor;
using JuegoSerio.Datos;
using JuegoSerio.IA;

namespace JuegoSerio.Demo
{
    // Encadena Motor de Cálculo -> ollama-unity para producir el contenido completo de la
    // Pantalla FINAL. Adjuntar a un GameObject vacío junto con OllamaReporteFinalGenerador.
    [RequireComponent(typeof(OllamaReporteFinalGenerador))]
    public class PruebaReporteFinal : MonoBehaviour
    {
        private async void Start()
        {
            var bancoEjes = CargadorBancoPreguntas.CargarBancoEjes().preguntas;
            var bancoSesgos = CargadorBancoPreguntas.CargarBancoSesgos().preguntas;
            var bancoDimensiones = CargadorBancoPreguntas.CargarBancoDimensiones().preguntas;

            var decision = new DecisionRegistro
            {
                ID_Proyecto = "PROY-01",
                Nombre_Decision = "Cambiar de motor de renderizado a mitad de producción",
                Contexto_Problema = "El equipo detectó cuellos de botella de rendimiento en la build vertical.",
                RespuestasEjes = new List<Respuesta>
                {
                    Respuesta.CrearEscala("INT_001", 3),
                    Respuesta.CrearSiNo("INT_002", false),
                    Respuesta.CrearEscala("INT_003", 2),
                    Respuesta.CrearSiNo("INT_004", true),
                    Respuesta.CrearEscala("ADA_001", 4),
                    Respuesta.CrearEscala("ADA_003", 5),
                    Respuesta.CrearSiNo("ADA_006", true),
                },
                RespuestasSesgos = new List<Respuesta>
                {
                    Respuesta.CrearSiNo("SESG_004", true),
                    Respuesta.CrearEscala("SESG_005", 4),
                },
                RespuestasDimensiones = new List<Respuesta>
                {
                    Respuesta.CrearSiNo("EST-01", true),
                    Respuesta.CrearSiNo("EST-02", true),
                    Respuesta.CrearSiNo("OPE-03", true),
                }
            };

            var resultado = OrquestadorMotorDecision.Evaluar(decision, bancoEjes, bancoSesgos, bancoDimensiones);
            string codigoTecnico = CodigoTecnicoGenerador.Generar(resultado);

            var generador = GetComponent<OllamaReporteFinalGenerador>();
            var reporte = await generador.GenerarAsync(decision, resultado);

            Debug.Log($"Código Técnico: {codigoTecnico}");
            Debug.Log($"Resultado final: {reporte.ResultadoFinal}");
            Debug.Log($"CTA: {reporte.CTA}");
        }
    }
}

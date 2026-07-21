using System.Collections.Generic;
using UnityEngine;
using JuegoSerio.Modelos;
using JuegoSerio.Motor;
using JuegoSerio.Datos;

namespace JuegoSerio.Demo
{
    // Ejemplo de uso de punta a punta. No es parte del motor: solo demuestra el flujo.
    // Adjuntar a cualquier GameObject vacío en una escena de prueba y correr Play.
    public class PruebaMotorDecision : MonoBehaviour
    {
        private void Start()
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
                    Respuesta.CrearSiNo("SESG_004", true),   // Anclaje de entrada
                    Respuesta.CrearEscala("SESG_005", 4),    // Exceso de confianza
                },
                RespuestasDimensiones = new List<Respuesta>
                {
                    Respuesta.CrearSiNo("EST-01", true),
                    Respuesta.CrearSiNo("EST-02", true),
                    Respuesta.CrearSiNo("OPE-03", true),
                }
            };

            var resultado = OrquestadorMotorDecision.Evaluar(decision, bancoEjes, bancoSesgos, bancoDimensiones);

            Debug.Log($"%Eficacia: {resultado.Termometro.PorcentajeEficacia:F1} | " +
                      $"%Eficiencia: {resultado.Termometro.PorcentajeEficiencia:F1}");
            Debug.Log($"Penalización por sesgos: {resultado.Sesgos.PenalizacionAcumulada:F2} " +
                      $"({string.Join(" | ", resultado.Sesgos.SesgosActivados)})");
            Debug.Log($"Índice de Salud Decisional: {resultado.IndiceSaludDecisional:F1}");
            Debug.Log($"Cuadrante: {resultado.Cuadrante} | Prioridad: {resultado.Prioridad} | " +
                      $"¿Pasa de fase?: {resultado.PasarSiguienteFase}");
            Debug.Log($"Tipos detectados: {string.Join(", ", resultado.TiposDetectados)}");
        }
    }
}

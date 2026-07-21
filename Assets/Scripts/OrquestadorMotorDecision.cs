using System.Collections.Generic;
using JuegoSerio.Modelos;

namespace JuegoSerio.Motor
{
    // Punto de entrada único del motor: corre los Bloques 2, 3 y 4 sobre una DecisionRegistro
    // y devuelve el paquete de datos listo para (a) mostrarse en la Pantalla FINAL y
    // (b) pasarse como contexto/JSON al LLM local vía ollama-unity para redactar el
    // "Resultado final" en lenguaje natural y el CTA (eso NO se hace en esta entrega).
    public static class OrquestadorMotorDecision
    {
        public static ResultadoDecisionCompleto Evaluar(
            DecisionRegistro decision,
            List<PreguntaEje> bancoEjes,
            List<PreguntaSesgo> bancoSesgos,
            List<PreguntaDimension> bancoDimensiones,
            float umbral = MotorPuntuacion.UMBRAL_EFICAZ_EFICIENTE)
        {
            var termometro = MotorPuntuacion.CalcularTermometro(bancoEjes, decision.RespuestasEjes, umbral);
            var sesgos = MotorPuntuacion.CalcularPenalizacionSesgos(bancoSesgos, decision.RespuestasSesgos);
            float indiceSalud = MotorPuntuacion.CalcularIndiceSaludDecisional(termometro, sesgos);
            var tipos = MotorPuntuacion.ClasificarTipoDecision(bancoDimensiones, decision.RespuestasDimensiones);
            var transicion = MotorPriorizacion.EvaluarPuenteMemoria(termometro.EsEficaz, termometro.EsEficiente);

            return new ResultadoDecisionCompleto
            {
                Termometro = termometro,
                Sesgos = sesgos,
                IndiceSaludDecisional = indiceSalud,
                Cuadrante = transicion.Cuadrante,
                Prioridad = transicion.Prioridad,
                PasarSiguienteFase = transicion.PasarSiguienteFase,
                TiposDetectados = tipos
            };
        }
    }
}

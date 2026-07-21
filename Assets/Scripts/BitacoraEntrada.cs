using System;
using JuegoSerio.Modelos;

namespace JuegoSerio.Juego
{
    // Una fila de la "Bitácora de Decisiones" (título del diagrama): una decisión ya cerrada
    // junto con todo lo que el motor y el LLM local produjeron para ella.
    [Serializable]
    public class BitacoraEntrada
    {
        public DecisionRegistro Decision;
        public ResultadoDecisionCompleto Resultado;
        public ReportePantallaFinal Reporte;
        public string CodigoTecnico;
    }
}

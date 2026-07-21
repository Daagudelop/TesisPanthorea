using System.Threading.Tasks;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Aísla la dependencia de ollama-unity detrás de una interfaz: si la firma de la librería
    // cambia entre versiones, o si más adelante cambian de proveedor de LLM local, solo se toca
    // la implementación (OllamaReporteFinalGenerador.cs), no el resto del juego.
    public interface IReporteFinalGenerador
    {
        Task<ReportePantallaFinal> GenerarAsync(DecisionRegistro decision, ResultadoDecisionCompleto resultado);
    }
}

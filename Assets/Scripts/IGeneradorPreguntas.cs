using System.Collections.Generic;
using System.Threading.Tasks;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Aísla la dependencia de ollama-unity, igual que IReporteFinalGenerador: si más adelante
    // cambian de proveedor de LLM, o quieren generar también Sesgos/Dimensiones, la firma no
    // cambia.
    public interface IGeneradorPreguntas
    {
        // 'bancoDeRespaldo' es el banco estático (banco_preguntas.json) — se devuelve tal cual
        // si la generación falla o el LLM no responde algo usable, para que el jugador nunca
        // se quede sin preguntas.
        Task<List<PreguntaEje>> GenerarPreguntasEjeAsync(DecisionRegistro decision, List<PreguntaEje> bancoDeRespaldo);
    }
}

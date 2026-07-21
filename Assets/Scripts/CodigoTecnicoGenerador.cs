using JuegoSerio.Modelos;

namespace JuegoSerio.Motor
{
    // "Código Técnico: clasificación alfanumérica para control interno del Serious Game,
    // que relaciona el frame del Avatar Clippy" (Arquitectura Pantalla Final).
    // Se genera aquí, en C#, y NO se le pide al LLM: es un valor que conduce lógica de juego
    // (qué frame de animación mostrar) y debe ser 100% determinista y reproducible.
    // Formato: EF{1|0}EI{1|0}-{Cuadrante A|B|C|D|N}. Ajustar el formato si el equipo de arte
    // ya tiene definida una nomenclatura propia para los frames de Clippy.
    public static class CodigoTecnicoGenerador
    {
        public static string Generar(ResultadoDecisionCompleto resultado)
        {
            string ef = resultado.Termometro.EsEficaz ? "EF1" : "EF0";
            string ei = resultado.Termometro.EsEficiente ? "EI1" : "EI0";
            string cuadrante = AbreviarCuadrante(resultado.Cuadrante);
            return $"{ef}{ei}-{cuadrante}";
        }

        private static string AbreviarCuadrante(CuadranteEisenhower c)
        {
            switch (c)
            {
                case CuadranteEisenhower.UrgenteImportante: return "A";
                case CuadranteEisenhower.NoUrgenteImportante: return "B";
                case CuadranteEisenhower.UrgenteNoImportante: return "C";
                case CuadranteEisenhower.NoUrgenteNoImportante: return "D";
                default: return "N"; // NoAplica (Caso 1: éxito total, se archiva)
            }
        }
    }
}

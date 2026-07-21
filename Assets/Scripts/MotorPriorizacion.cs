using JuegoSerio.Modelos;

namespace JuegoSerio.Motor
{
    // Bloque 3 (Matriz de Priorización / Eisenhower) + Puente de Memoria.
    //
    // La lógica de los 4 casos reproduce EXACTAMENTE puente_de_memoria.txt.
    // El mapeo de cada caso a los cuadrantes A/B/C/D del diagrama de flujo es una
    // interpretación razonada mía (no viene explícita en ningún documento fuente):
    //
    //   Caso 1  eficaz && eficiente     -> No pasa de fase, se archiva (no entra a la matriz)
    //   Caso 2 !eficaz &&  eficiente    -> Cuadrante C (Urgente / No Importante): "error de
    //                                      enfoque", recursos bien usados pero sin cumplir el
    //                                      objetivo -> simplificar/delegar con ruta sencilla.
    //   Caso 3 !eficaz && !eficiente    -> Cuadrante A (Urgente e Importante), Prioridad Alta:
    //                                      alerta crítica, pasa de inmediato a Fase Táctica.
    //   Caso 4  eficaz && !eficiente    -> Cuadrante B (No Urgente e Importante), Prioridad
    //                                      Recomendada: costosa pero sirvió, reevaluar recursos.
    //
    // Revisar esta correspondencia con el equipo de diseño antes de darla por definitiva.
    public static class MotorPriorizacion
    {
        public struct DecisionTransicion
        {
            public bool PasarSiguienteFase;
            public PrioridadTransicion Prioridad;
            public CuadranteEisenhower Cuadrante;
            public string Explicacion;
        }

        public static DecisionTransicion EvaluarPuenteMemoria(bool esEficaz, bool esEficiente)
        {
            if (esEficaz && esEficiente)
            {
                // Caso 1: Éxito total, se archiva.
                return new DecisionTransicion
                {
                    PasarSiguienteFase = false,
                    Prioridad = PrioridadTransicion.Ninguna,
                    Cuadrante = CuadranteEisenhower.NoAplica,
                    Explicacion = "Éxito total: decisión eficaz y eficiente. Se archiva sin pasar a la siguiente fase."
                };
            }
            else if (!esEficaz && esEficiente)
            {
                // Caso 2: Error de enfoque, debe reevaluarse.
                return new DecisionTransicion
                {
                    PasarSiguienteFase = true,
                    Prioridad = PrioridadTransicion.Ninguna,
                    Cuadrante = CuadranteEisenhower.UrgenteNoImportante,
                    Explicacion = "Error de enfoque: los recursos se usaron bien pero no se cumplió el objetivo. " +
                                  "Simplificar/delegar en Fase Táctica con una ruta sencilla."
                };
            }
            else if (!esEficaz && !esEficiente)
            {
                // Caso 3: Alerta crítica, pasa INMEDIATAMENTE.
                return new DecisionTransicion
                {
                    PasarSiguienteFase = true,
                    Prioridad = PrioridadTransicion.Alta,
                    Cuadrante = CuadranteEisenhower.UrgenteImportante,
                    Explicacion = "Alerta crítica: ni eficaz ni eficiente. Pasa de inmediato a Fase Táctica " +
                                  "para corregir la alineación con los objetivos."
                };
            }
            else // esEficaz && !esEficiente
            {
                // Caso 4: Costosa pero sirvió, se recomienda reevaluar recursos.
                return new DecisionTransicion
                {
                    PasarSiguienteFase = true,
                    Prioridad = PrioridadTransicion.Recomendada,
                    Cuadrante = CuadranteEisenhower.NoUrgenteImportante,
                    Explicacion = "Costosa pero efectiva: cumplió el objetivo con recursos excesivos. " +
                                  "Se recomienda planificar rutas alternativas sostenibles en Fase Táctica."
                };
            }
        }
    }
}

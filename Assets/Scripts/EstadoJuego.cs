namespace JuegoSerio.Juego
{
    // Refleja los bloques del diagrama "Flujo del juego serio, Bitácora de Decisiones".
    public enum EstadoJuego
    {
        RegistroInicial,      // Bloque 1: ID_Proyecto, Nombre_Decision, Contexto_Problema
        GenerandoPreguntas,   // Opcional: el LLM local inventa el banco del Termómetro para esta decisión
        ResponderTermometro,  // Bloque 2: preguntas de Eficacia/Eficiencia (banco_preguntas.json o generadas por IA)
        ResponderSesgos,      // Bloque 4 (preguntas): Modificadores_Sesgos (banco_sesgos.json)
        ResponderDimensiones, // Diccionario_Dimensiones: clasifica Estratégica/Operativa/Creativa
        Calculando,           // Motor de cálculo + llamada al LLM local (ollama-unity)
        PantallaFinal,        // Resultado final + Código Técnico + CTA
        FinDeSesion            // Sin más decisiones que registrar
    }
}

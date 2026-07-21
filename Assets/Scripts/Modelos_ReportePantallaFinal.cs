using System;

namespace JuegoSerio.Modelos
{
    // Lo que el LLM local (ollama-unity) debe devolver para la Pantalla FINAL.
    // Solo texto: los números y el Código_Técnico se calculan en C#, nunca por el modelo.
    [Serializable]
    public class ReportePantallaFinal
    {
        public string ResultadoFinal; // "Resultado final" — texto explicativo (ej. "Tu decisión fue Efectiva pero Altamente Ineficiente.")
        public string CTA;            // "Llamado a la Acción" — siguiente paso interactivo en el juego
    }
}

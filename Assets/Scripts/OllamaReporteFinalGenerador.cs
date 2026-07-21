using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ollama;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Envuelve ollama-unity (https://github.com/Haoming02/ollama-unity) para redactar el
    // "Resultado final" y el "CTA" de la Pantalla FINAL.
    //
    // API confirmada contra el ejemplo real DemoChat.cs de ollama-unity — ya NO se usa
    // GenerateJson() (nunca pude verificar su firma): en su lugar se abre una sesión de chat
    // aislada con InitChat(), se le pide al modelo que responda ÚNICAMENTE con JSON vía
    // ChatStream(), y el JSON se extrae/parsea en C# con JsonUtility.
    //
    // Requisitos:
    //  1. El paquete/.unitypackage de ollama-unity importado al proyecto (namespace "ollama").
    //  2. Un servidor ollama con el modelo ya descargado (ej. `ollama pull gemma3:4b`) —
    //     Ollama.Launch() lo arranca por código, ver OllamaBootstrap.cs.
    public class OllamaReporteFinalGenerador : MonoBehaviour, IReporteFinalGenerador
    {
        [Tooltip("Debe existir localmente: ollama pull <modelo>")]
        [SerializeField] private string modelo = "gemma3:4b";

        public async Task<ReportePantallaFinal> GenerarAsync(DecisionRegistro decision, ResultadoDecisionCompleto resultado)
        {
            string prompt = ConstruirPrompt(decision, resultado);
            ReportePantallaFinal reporte = null;

            try
            {
                OllamaBootstrap.AsegurarLanzado();
                Ollama.InitChat(); // sesión de chat nueva y aislada, solo para este reporte

                var acumulado = new StringBuilder();
                await Ollama.ChatStream(fragmento => acumulado.Append(fragmento), modelo, prompt);
                reporte = ParsearJson(acumulado.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JuegoSerio] Fallo al consultar el LLM local ({ex.Message}). Usando texto de respaldo.");
            }

            if (reporte == null || string.IsNullOrEmpty(reporte.ResultadoFinal))
                reporte = GenerarRespaldo(resultado);

            return reporte;
        }

        // El modelo a veces agrega texto alrededor o code fences (```json ... ```) aunque se le
        // pida solo JSON; se extrae el primer bloque { ... } antes de parsear, en vez de asumir
        // que toda la respuesta es JSON puro.
        private ReportePantallaFinal ParsearJson(string textoCrudo)
        {
            int inicio = textoCrudo.IndexOf('{');
            int fin = textoCrudo.LastIndexOf('}');
            if (inicio < 0 || fin <= inicio) return null;

            string json = textoCrudo.Substring(inicio, fin - inicio + 1);
            try { return JsonUtility.FromJson<ReportePantallaFinal>(json); }
            catch { return null; }
        }

        // Le pasamos TODOS los valores ya calculados por el motor (Bloques 2/3/4). El modelo
        // solo redacta — tiene prohibido recalcular o inventar cualquier número.
        private string ConstruirPrompt(DecisionRegistro decision, ResultadoDecisionCompleto resultado)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Eres el asistente narrativo de un juego serio sobre toma de decisiones en colectivos creativos.");
            sb.AppendLine("A continuación tienes los resultados YA CALCULADOS de una decisión registrada por el equipo.");
            sb.AppendLine("NO recalcules ni inventes ningún número: solo redacta con base en los datos dados.");
            sb.AppendLine();
            sb.AppendLine($"Proyecto: {decision.ID_Proyecto}");
            sb.AppendLine($"Decisión: {decision.Nombre_Decision}");
            sb.AppendLine($"Contexto: {decision.Contexto_Problema}");
            sb.AppendLine($"% Eficacia: {resultado.Termometro.PorcentajeEficacia:F1}");
            sb.AppendLine($"% Eficiencia: {resultado.Termometro.PorcentajeEficiencia:F1}");
            sb.AppendLine($"Índice de Salud Decisional: {resultado.IndiceSaludDecisional:F1}");
            sb.AppendLine($"Cuadrante de priorización: {resultado.Cuadrante}");
            sb.AppendLine($"Prioridad de transición: {resultado.Prioridad}");
            sb.AppendLine($"¿Pasa a la siguiente fase?: {(resultado.PasarSiguienteFase ? "Sí" : "No")}");

            if (resultado.Sesgos.SesgosActivados.Count > 0)
                sb.AppendLine($"Sesgos cognitivos detectados: {string.Join("; ", resultado.Sesgos.SesgosActivados)}");

            if (resultado.TiposDetectados.Count > 0)
                sb.AppendLine($"Tipo(s) de decisión: {string.Join(", ", resultado.TiposDetectados)}");

            sb.AppendLine();
            sb.AppendLine("Responde ÚNICAMENTE con un JSON válido, sin texto adicional antes ni después, con este formato exacto:");
            sb.AppendLine("{\"ResultadoFinal\": \"<1-2 frases explicando el diagnóstico, ej. 'Tu decisión fue Efectiva pero Altamente Ineficiente.'>\", " +
                           "\"CTA\": \"<1 frase indicando el siguiente paso interactivo en el juego>\"}");
            return sb.ToString();
        }

        // Respaldo determinista si el LLM local no responde o el JSON no parsea — así la
        // Pantalla FINAL nunca se queda sin texto aunque ollama no esté corriendo.
        private ReportePantallaFinal GenerarRespaldo(ResultadoDecisionCompleto resultado)
        {
            string eficacia = resultado.Termometro.EsEficaz ? "Efectiva" : "Poco Efectiva";
            string eficiencia = resultado.Termometro.EsEficiente ? "Eficiente" : "Altamente Ineficiente";

            return new ReportePantallaFinal
            {
                ResultadoFinal = $"Tu decisión fue {eficacia} y {eficiencia}.",
                CTA = resultado.PasarSiguienteFase
                    ? "Ir a reconfigurar la Fase Táctica."
                    : "Decisión archivada: continúa con la siguiente."
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ollama;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Genera dinámicamente el banco del Termómetro (Bloque 2) a partir del contexto de la
    // decisión, en vez de usar siempre banco_preguntas.json.
    //
    // Reutiliza EXACTAMENTE las mismas clases que el banco estático (BancoPreguntasEje /
    // PreguntaEje), así que MotorPuntuacion no necesita ningún cambio: para el motor da igual
    // si las preguntas vinieron de un archivo o las inventó el LLM local en este momento.
    //
    // Dos decisiones de diseño que esto resuelve de paso:
    //  - Polaridad: se le pide al modelo que redacte SIEMPRE las preguntas Sí/No de forma que
    //    "Sí" sea la respuesta favorable. Elimina la ambigüedad que sí tuvimos que inferir a
    //    mano en el banco estático original (ver README_MotorCalculo.md).
    //  - Pesos: CalcularTermometro() normaliza dividiendo por la suma de los pesos, así que es
    //    matemáticamente válido sin importar la escala que el modelo use — no hace falta que
    //    Peso_Eficacia + Peso_Eficiencia sumen 1 ni nada parecido.
    public class OllamaGeneradorPreguntas : MonoBehaviour, IGeneradorPreguntas
    {
        [Tooltip("Debe existir localmente: ollama pull <modelo>")]
        [SerializeField] private string modelo = "gemma3:4b";
        [SerializeField] private int cantidadPreguntas = 6;

        public async Task<List<PreguntaEje>> GenerarPreguntasEjeAsync(DecisionRegistro decision, List<PreguntaEje> bancoDeRespaldo)
        {
            try
            {
                OllamaBootstrap.AsegurarLanzado();
                Ollama.InitChat(); // sesión aislada, solo para generar este banco

                string prompt = ConstruirPrompt(decision);
                var acumulado = new StringBuilder();
                var tarea = Ollama.ChatStream(fragmento => acumulado.Append(fragmento), modelo, prompt);
                await OllamaBootstrap.EsperarConTimeout(tarea, 100000f,
                    $"El modelo local '{modelo}' no respondió a tiempo generando las preguntas.");

                var banco = ParsearBanco(acumulado.ToString());
                if (banco != null && banco.Count > 0)
                {
                    Debug.Log($"[JuegoSerio] {banco.Count} preguntas generadas por el LLM local para esta decisión.");
                    return banco;
                }

                Debug.LogWarning("[JuegoSerio] El LLM no devolvió preguntas válidas; se usa el banco fijo de respaldo.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JuegoSerio] Fallo generando preguntas con el LLM local ({ex.Message}). Se usa el banco fijo de respaldo.");
            }

            return bancoDeRespaldo;
        }

        // Mismo patrón que OllamaReporteFinalGenerador: se extrae el primer bloque { ... }
        // antes de parsear, por si el modelo agrega texto o code fences alrededor del JSON.
        private List<PreguntaEje> ParsearBanco(string textoCrudo)
        {
            int inicio = textoCrudo.IndexOf('{');
            int fin = textoCrudo.LastIndexOf('}');
            if (inicio < 0 || fin <= inicio) return null;

            string json = textoCrudo.Substring(inicio, fin - inicio + 1);
            try
            {
                var banco = JsonUtility.FromJson<BancoPreguntasEje>(json);
                return banco?.preguntas;
            }
            catch
            {
                return null;
            }
        }

        private string ConstruirPrompt(DecisionRegistro decision)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Eres un diseñador de evaluaciones dentro de un juego serio sobre toma de decisiones en colectivos creativos.");
            sb.AppendLine($"Genera exactamente {cantidadPreguntas} preguntas cerradas para evaluar la EFICACIA y la EFICIENCIA de esta decisión concreta (no preguntas genéricas):");
            sb.AppendLine($"Decisión: {decision.Nombre_Decision}");
            sb.AppendLine($"Contexto: {decision.Contexto_Problema}");
            sb.AppendLine();
            sb.AppendLine("Reglas:");
            sb.AppendLine("- Cada pregunta es de tipo \"Escala1a5\" (acuerdo/desacuerdo, donde 5 = mejor resultado) o \"SiNo\".");
            sb.AppendLine("- En las preguntas \"SiNo\", redacta SIEMPRE de forma que \"Sí\" sea la respuesta favorable/deseable. Pon \"Polaridad\": \"Positiva\" en todas.");
            sb.AppendLine("- Cada pregunta evalúa Eficacia (impacto en el objetivo), Eficiencia (uso de tiempo/equipo/presupuesto), o ambas — asigna Peso_Eficacia y Peso_Eficiencia entre 0 y 1 según cuánto mida cada una.");
            sb.AppendLine("- El campo Eje_Tematico debe ser \"Interna\" (recursos propios del equipo), \"Relacional\" (alianzas/compromisos externos) o \"Adaptativa\" (manejo de imprevistos).");
            sb.AppendLine("- ID_Pregunta debe ser único y corto, ej. \"Q1\", \"Q2\"...");
            sb.AppendLine();
            sb.AppendLine("Responde ÚNICAMENTE con un JSON válido, sin texto adicional antes ni después, con este formato exacto:");
            sb.AppendLine("{\"preguntas\": [{\"ID_Pregunta\": \"Q1\", \"Eje_Tematico\": \"Interna\", \"Tipo_Pregunta\": \"Escala1a5\", " +
                           "\"Criterio_Asociado\": \"...\", \"Texto_Pregunta\": \"...\", \"Peso_Eficacia\": 0.7, \"Peso_Eficiencia\": 0.3, \"Polaridad\": \"Positiva\"}, ...]}");
            return sb.ToString();
        }
    }
}

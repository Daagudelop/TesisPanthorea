using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ollama;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Facilitador conversacional del Bloque 1: "Si el usuario no tiene claras las decisiones,
    // la IA actúa como facilitadora con preguntas directas" + "IA reorganiza la información
    // registrada y realiza preguntas en caso de ser necesario".
    //
    // API confirmada contra el ejemplo real DemoChat.cs de ollama-unity:
    //   Ollama.Launch()                                  -> arranca el servidor local (una vez)
    //   Ollama.InitChat()                                -> SIN parámetros, arranca una sesión de chat
    //   Ollama.ChatStream(Action<string> cb, model, msg)  -> awaitable, transmite la respuesta en fragmentos
    //
    // InitChat() no acepta un system prompt en esta versión, así que lo sembramos como el
    // primer turno de la conversación y devolvemos la respuesta del modelo a ese primer turno
    // (su saludo/pregunta inicial) para que la UI la muestre de entrada.
    //
    // A propósito NO se usa ChatWithTool() ni GenerateJson(): no aparecen en el ejemplo
    // confirmado, así que en vez de arriesgar una firma no verificada, uso únicamente
    // ChatStream() (sí confirmado) y hago el parseo/extracción en C#.
    public class FacilitadorConversacional : MonoBehaviour
    {
        [Tooltip("Debe existir localmente: ollama pull <modelo>")]
        [SerializeField] private string modelo = "gemma3:4b";

        private const string Marcador = "REGISTRO_LISTO:";

        // Se dispara cuando el facilitador ya reunió Nombre_Decision + Contexto_Problema.
        public event Action<DecisionRegistro> OnDecisionExtraida;

        private bool inicializado;
        private string idProyectoSesion;

        // Devuelve el saludo/pregunta inicial del facilitador, para mostrarlo de entrada en el chat.
        public async Task<string> IniciarConversacion(string idProyecto)
        {
            idProyectoSesion = idProyecto;

            OllamaBootstrap.AsegurarLanzado();
            Ollama.InitChat();

            string saludoInicial = await EnviarYEsperarCompleto(ConstruirPromptSistema());
            inicializado = true;
            return saludoInicial;
        }

        // Envía el mensaje del jugador y devuelve SOLO la parte conversacional para mostrar
        // en pantalla. Si el modelo ya incluyó el marcador de registro, se extrae aparte y
        // dispara OnDecisionExtraida — no se muestra el marcador crudo al jugador.
        public async Task<string> EnviarMensaje(string mensajeUsuario)
        {
            if (!inicializado)
            {
                Debug.LogError("[JuegoSerio] Llama a IniciarConversacion() antes de EnviarMensaje().");
                return string.Empty;
            }

            string respuesta;
            try
            {
                respuesta = await EnviarYEsperarCompleto(mensajeUsuario);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JuegoSerio] Fallo al consultar el LLM local ({ex.Message}).");
                return "No pude conectarme al asistente local en este momento. ¿Puedes describir tu decisión con tus propias palabras mientras lo resolvemos?";
            }

            string textoConversacional = respuesta;
            int indiceMarcador = respuesta.IndexOf(Marcador, StringComparison.Ordinal);
            if (indiceMarcador >= 0)
            {
                textoConversacional = respuesta.Substring(0, indiceMarcador).Trim();
                string jsonCrudo = respuesta.Substring(indiceMarcador + Marcador.Length).Trim();
                TryExtraerDecision(jsonCrudo);
            }

            return textoConversacional;
        }

        // ollama-unity transmite la respuesta en fragmentos vía callback; los acumulamos hasta
        // que el await se completa (la Task se resuelve cuando el streaming termina).
        private async Task<string> EnviarYEsperarCompleto(string mensaje)
        {
            var acumulado = new StringBuilder();
            await Ollama.ChatStream(fragmento => acumulado.Append(fragmento), modelo, mensaje);
            return acumulado.ToString();
        }

        private void TryExtraerDecision(string jsonCrudo)
        {
            try
            {
                var extraido = JsonUtility.FromJson<DecisionExtraidaJSON>(jsonCrudo);
                if (string.IsNullOrEmpty(extraido?.Nombre_Decision) || string.IsNullOrEmpty(extraido.Contexto_Problema))
                {
                    Debug.LogWarning("[JuegoSerio] El marcador de registro llegó incompleto, se ignora.");
                    return;
                }

                OnDecisionExtraida?.Invoke(new DecisionRegistro
                {
                    ID_Proyecto = idProyectoSesion,
                    Nombre_Decision = extraido.Nombre_Decision,
                    Contexto_Problema = extraido.Contexto_Problema
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JuegoSerio] No se pudo parsear el JSON del marcador de registro: {ex.Message}");
            }
        }

        [Serializable]
        private class DecisionExtraidaJSON
        {
            public string Nombre_Decision;
            public string Contexto_Problema;
        }

        private string ConstruirPromptSistema()
        {
            return
                "Eres un facilitador breve y directo dentro de un juego serio sobre toma de decisiones " +
                "en colectivos creativos. A partir de ahora actúas ese rol para toda la conversación. " +
                "Tu única tarea es ayudar al jugador a identificar UNA decisión concreta que su equipo ya " +
                "tomó en el proyecto, si es que no la tiene clara, mediante preguntas cortas y directas " +
                "(una a la vez, sin abrumar). Empieza ahora mismo preguntándole por su decisión.\n" +
                "Necesitas obtener dos datos: (1) un nombre corto para la decisión, y (2) el contexto o " +
                "problema que la originó.\n" +
                "En cuanto tengas ambos datos con claridad suficiente, agrega SIEMPRE al final de tu " +
                "respuesta (después de tu texto normal para el jugador) una línea nueva exactamente así, " +
                "sin nada más después:\n" +
                "REGISTRO_LISTO:{\"Nombre_Decision\": \"...\", \"Contexto_Problema\": \"...\"}\n" +
                "Esa línea es para el sistema, no la anuncies ni la expliques al jugador. Si todavía no " +
                "tienes ambos datos con claridad, NO incluyas esa línea y sigue preguntando.";
        }
    }
}

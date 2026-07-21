using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ollama;
using JuegoSerio.Modelos;

namespace JuegoSerio.IA
{
    // Confirmado contra el ejemplo real DemoChat.cs de ollama-unity: Ollama.Launch() debe
    // llamarse una vez (ellos lo hacen en Awake()) antes de usar InitChat()/ChatStream().
    // Como acá dos componentes distintos (FacilitadorConversacional y
    // OllamaReporteFinalGenerador) pueden necesitar el servidor arrancado, centralizo la
    // llamada aquí con una bandera para no invocar Launch() más de una vez por sesión.
    public static class OllamaBootstrap
    {
        private static bool lanzado;

        public static void AsegurarLanzado()
        {
            if (lanzado) return;
            Ollama.Launch();
            lanzado = true;
        }
        // Envuelve una tarea del LLM con un timeout: si "tarea" no termina antes de
        // "timeoutSegundos", se lanza TimeoutException con el mensaje dado. Si "tarea"
        // termina primero, se propaga cualquier excepción que haya tenido (await tarea
        // vuelve a "abrir" la tarea ya completada y tira su excepción original si la hubo).
        public static async Task EsperarConTimeout(Task tarea, float timeoutSegundos, string mensajeTimeout)
        {
            var tareaTimeout = Task.Delay(TimeSpan.FromSeconds(timeoutSegundos));
            var completada = await Task.WhenAny(tarea, tareaTimeout);

            if (completada == tareaTimeout)
            {
                throw new TimeoutException(mensajeTimeout);
            }

            // Re-await para propagar la excepción original si "tarea" falló en vez de tardar.
            await tarea;
        }
    }
}

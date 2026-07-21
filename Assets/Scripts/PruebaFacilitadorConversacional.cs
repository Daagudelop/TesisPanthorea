using UnityEngine;
using JuegoSerio.IA;
using JuegoSerio.Juego;

namespace JuegoSerio.Demo
{
    // Simula una conversación con el facilitador (Bloque 1) cuando el jugador no tiene claras
    // sus decisiones. Requiere un GameObject con FacilitadorConversacional en la escena, y
    // opcionalmente un GameManager con el campo "facilitador" asignado a ese mismo componente
    // para ver cómo IniciarNuevaDecision() se dispara solo al terminar la charla.
    public class PruebaFacilitadorConversacional : MonoBehaviour
    {
        [SerializeField] private FacilitadorConversacional facilitador;

        private async void Start()
        {
            if (facilitador == null) facilitador = GetComponent<FacilitadorConversacional>();

            string saludoInicial = await facilitador.IniciarConversacion("PROY-01");
            Debug.Log($"[Facilitador] {saludoInicial}");

            // Turnos simulados de un jugador que "no tiene clara" su decisión todavía.
            string[] mensajesDelJugador =
            {
                "No sé, tomamos varias decisiones esta semana, no sabría por cuál empezar.",
                "Bueno, cambiamos de motor de renderizado a mitad de producción.",
                "Fue porque teníamos cuellos de botella de rendimiento en la build vertical y el equipo de arte no podía iterar rápido."
            };

            foreach (var mensaje in mensajesDelJugador)
            {
                Debug.Log($"[Jugador] {mensaje}");
                string respuesta = await facilitador.EnviarMensaje(mensaje);
                Debug.Log($"[Facilitador] {respuesta}");
            }

            // Si el modelo ya reunió los datos, GameManager.Instancia.Estado ya debería haber
            // pasado de RegistroInicial a ResponderTermometro sin que nadie más lo dispare.
            if (GameManager.Instancia != null)
                Debug.Log($"[GameManager] Estado tras la conversación: {GameManager.Instancia.Estado}");
        }
    }
}

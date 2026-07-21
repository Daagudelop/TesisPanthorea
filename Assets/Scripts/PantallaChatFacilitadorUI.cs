using System.Text;
using UnityEngine;
using TMPro;
using JuegoSerio.IA;

namespace JuegoSerio.UI
{
    // Bloque 1 — alternativa conversacional a PantallaRegistroUI: el jugador escribe libremente
    // y el facilitador (FacilitadorConversacional) hace las preguntas necesarias hasta reunir
    // Nombre_Decision + Contexto_Problema, momento en el que GameManager.IniciarNuevaDecision()
    // se dispara solo (ver el campo "facilitador" en GameManager).
    public class PantallaChatFacilitadorUI : MonoBehaviour
    {
        [SerializeField] private FacilitadorConversacional facilitador;
        [SerializeField] private TMP_Text historialChat;
        [SerializeField] private TMP_InputField campoMensaje;
        [SerializeField] private string idProyectoSesion = "PROY-01";

        private readonly StringBuilder historial = new StringBuilder();
        private bool conversacionIniciada;

        private async void OnEnable()
        {
            if (conversacionIniciada) return;
            conversacionIniciada = true;
            string saludoInicial = await facilitador.IniciarConversacion(idProyectoSesion);
            AgregarLinea("Facilitador", saludoInicial);
        }

        // Conectar al botón "Enviar" en el Inspector.
        public async void OnClickEnviar()
        {
            string mensaje = campoMensaje.text;
            if (string.IsNullOrWhiteSpace(mensaje)) return;

            AgregarLinea("Tú", mensaje);
            campoMensaje.text = string.Empty;
            campoMensaje.interactable = false;

            string respuesta = await facilitador.EnviarMensaje(mensaje);
            AgregarLinea("Facilitador", respuesta);

            campoMensaje.interactable = true;
        }

        private void AgregarLinea(string quien, string texto)
        {
            historial.AppendLine($"<b>{quien}:</b> {texto}");
            historialChat.text = historial.ToString();
        }
    }
}

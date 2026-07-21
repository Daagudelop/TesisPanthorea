using UnityEngine;
using TMPro;
using JuegoSerio.Juego;

namespace JuegoSerio.UI
{
    // Bloque 1 — formulario clásico. Alternativa a PantallaChatFacilitadorUI: usa esta si NO
    // quieres que el jugador converse con el LLM local para identificar la decisión, y prefieres
    // 3 campos de texto directos.
    public class PantallaRegistroUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField campoIdProyecto;
        [SerializeField] private TMP_InputField campoNombreDecision;
        [SerializeField] private TMP_InputField campoContexto;

        private void OnEnable()
        {
            // Recuerda el último ID de proyecto usado, para no reescribirlo en cada decisión.
            if (GameManager.Instancia != null && GameManager.Instancia.Bitacora.Count > 0)
                campoIdProyecto.text = GameManager.Instancia.Bitacora[GameManager.Instancia.Bitacora.Count - 1].Decision.ID_Proyecto;
        }

        // Conectar al botón "Registrar decisión" en el Inspector.
        public void OnClickRegistrarDecision()
        {
            if (string.IsNullOrWhiteSpace(campoNombreDecision.text) || string.IsNullOrWhiteSpace(campoContexto.text))
            {
                Debug.LogWarning("[JuegoSerio] Completa nombre y contexto antes de continuar.");
                return;
            }

            GameManager.Instancia.IniciarNuevaDecision(
                campoIdProyecto.text,
                campoNombreDecision.text,
                campoContexto.text);
        }
    }
}

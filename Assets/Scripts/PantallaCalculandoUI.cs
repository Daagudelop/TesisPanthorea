using System.Collections;
using UnityEngine;
using TMPro;

namespace JuegoSerio.UI
{
    // Pantalla de espera mientras el motor calcula y el LLM local redacta el reporte
    // (estado EstadoJuego.Calculando). Puramente decorativa, no llama a nada del GameManager.
    public class PantallaCalculandoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text texto;
        [SerializeField] private string mensajeBase = "Calculando diagnóstico";

        private Coroutine animacion;

        private void OnEnable() => animacion = StartCoroutine(AnimarPuntos());
        private void OnDisable() { if (animacion != null) StopCoroutine(animacion); }

        private IEnumerator AnimarPuntos()
        {
            int puntos = 0;
            while (true)
            {
                puntos = (puntos + 1) % 4;
                texto.text = mensajeBase + new string('.', puntos);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}

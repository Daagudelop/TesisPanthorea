using UnityEngine;
using TMPro;
using JuegoSerio.Modelos;
using JuegoSerio.Juego;

namespace JuegoSerio.UI
{
    // Pantalla FINAL: %Eficacia/%Eficiencia, Índice de Salud Decisional, Cuadrante, Prioridad,
    // Código Técnico (el que conduce el frame del Avatar Clippy), y el texto redactado por el
    // LLM local (ResultadoFinal + CTA).
    public class PantallaFinalUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text textoEficaciaEficiencia;
        [SerializeField] private TMP_Text textoIndiceSalud;
        [SerializeField] private TMP_Text textoCuadrantePrioridad;
        [SerializeField] private TMP_Text textoCodigoTecnico;
        [SerializeField] private TMP_Text textoResultadoFinal;
        [SerializeField] private TMP_Text textoCTA;

        // Llamado por ControladorPantallas cuando GameManager dispara OnReporteListo.
        public void MostrarResultado(ResultadoDecisionCompleto resultado, ReportePantallaFinal reporte, string codigoTecnico)
        {
            textoEficaciaEficiencia.text =
                $"Eficacia: {resultado.Termometro.PorcentajeEficacia:F0}%   Eficiencia: {resultado.Termometro.PorcentajeEficiencia:F0}%";
            textoIndiceSalud.text = $"Índice de Salud Decisional: {resultado.IndiceSaludDecisional:F0}";
            textoCuadrantePrioridad.text = $"Cuadrante: {resultado.Cuadrante}   Prioridad: {resultado.Prioridad}";
            textoCodigoTecnico.text = $"Código Técnico: {codigoTecnico}";
            textoResultadoFinal.text = reporte.ResultadoFinal;
            textoCTA.text = reporte.CTA;
        }

        // Conectar a los botones "Registrar otra decisión" / "Terminar sesión" en el Inspector.
        public void OnClickRegistrarOtraDecision() => GameManager.Instancia.RegistrarOtraDecision();
        public void OnClickTerminarSesion() => GameManager.Instancia.TerminarSesion();
    }
}

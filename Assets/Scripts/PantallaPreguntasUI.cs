using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JuegoSerio.Modelos;
using JuegoSerio.Juego;

namespace JuegoSerio.UI
{
    public enum BloquePreguntas { Termometro, Sesgos, Dimensiones }

    // Panel único y reutilizable: recorre CUALQUIERA de los 3 bancos de preguntas cerradas
    // (Termómetro, Sesgos, Dimensiones) una pregunta a la vez, y avisa al GameManager correcto
    // según el tipo de respuesta (Escala 1-5 o Sí/No) y el bloque activo.
    public class PantallaPreguntasUI : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text textoPregunta;
        [SerializeField] private TMP_Text textoProgreso;

        [Header("Controles de respuesta (grupos de botones)")]
        [SerializeField] private GameObject panelEscala; // contiene 5 botones que llaman OnResponderEscala(1..5)
        [SerializeField] private GameObject panelSiNo;    // contiene 2 botones: Sí -> OnResponderSiNo(true), No -> OnResponderSiNo(false)

        private struct PreguntaGenerica
        {
            public string Id;
            public string Texto;
            public bool EsSiNo;
        }

        private readonly List<PreguntaGenerica> preguntas = new List<PreguntaGenerica>();
        private int indiceActual;
        private BloquePreguntas bloqueActual;

        // Llamado por ControladorPantallas al entrar a ResponderTermometro/Sesgos/Dimensiones.
        public void MostrarBloque(BloquePreguntas bloque)
        {
            bloqueActual = bloque;
            preguntas.Clear();

            switch (bloque)
            {
                case BloquePreguntas.Termometro:
                    foreach (var p in GameManager.Instancia.BancoEjes)
                        preguntas.Add(new PreguntaGenerica { Id = p.ID_Pregunta, Texto = p.Texto_Pregunta, EsSiNo = p.Tipo_Pregunta == "SiNo" });
                    break;
                case BloquePreguntas.Sesgos:
                    foreach (var p in GameManager.Instancia.BancoSesgos)
                        preguntas.Add(new PreguntaGenerica { Id = p.ID_Pregunta, Texto = p.Texto_Pregunta, EsSiNo = p.Tipo_Pregunta == "SiNo" });
                    break;
                case BloquePreguntas.Dimensiones:
                    // El Diccionario_Dimensiones siempre es Sí/No; el ID que se registra es
                    // el Codigo_Tipo (EST-01, OPE-03, ...), no un ID_Pregunta propio.
                    foreach (var p in GameManager.Instancia.BancoDimensiones)
                        preguntas.Add(new PreguntaGenerica { Id = p.Codigo_Tipo, Texto = p.Pregunta_Guia, EsSiNo = true });
                    break;
            }

            indiceActual = 0;
            MostrarPreguntaActual();
        }

        private void MostrarPreguntaActual()
        {
            if (indiceActual >= preguntas.Count)
            {
                TerminarBloqueActual();
                return;
            }

            var p = preguntas[indiceActual];
            textoPregunta.text = p.Texto;
            textoProgreso.text = $"{indiceActual + 1}/{preguntas.Count}";

            panelEscala.SetActive(!p.EsSiNo);
            panelSiNo.SetActive(p.EsSiNo);
        }

        // Conectar en el Inspector: 5 botones de escala (OnClick -> OnResponderEscala, pasando 1-5).
        public void OnResponderEscala(int valor)
        {
            Respuesta respuesta = Respuesta.CrearEscala(preguntas[indiceActual].Id, valor);
            RegistrarEnGameManager(respuesta);
            Avanzar();
        }

        // Conectar en el Inspector: 2 botones Sí/No (OnClick -> OnResponderSiNo, pasando true/false).
        public void OnResponderSiNo(bool valor)
        {
            Respuesta respuesta = Respuesta.CrearSiNo(preguntas[indiceActual].Id, valor);
            RegistrarEnGameManager(respuesta);
            Avanzar();
        }

        private void RegistrarEnGameManager(Respuesta respuesta)
        {
            switch (bloqueActual)
            {
                case BloquePreguntas.Termometro: GameManager.Instancia.RegistrarRespuestaEje(respuesta); break;
                case BloquePreguntas.Sesgos: GameManager.Instancia.RegistrarRespuestaSesgo(respuesta); break;
                case BloquePreguntas.Dimensiones: GameManager.Instancia.RegistrarRespuestaDimension(respuesta); break;
            }
        }

        private void Avanzar()
        {
            indiceActual++;
            MostrarPreguntaActual();
        }

        private void TerminarBloqueActual()
        {
            switch (bloqueActual)
            {
                case BloquePreguntas.Termometro: GameManager.Instancia.TerminarTermometro(); break;
                case BloquePreguntas.Sesgos: GameManager.Instancia.TerminarSesgos(); break;
                case BloquePreguntas.Dimensiones: GameManager.Instancia.TerminarDimensionesYCalcular(); break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using JuegoSerio.Modelos;

namespace JuegoSerio.Motor
{
    // Bloque 2 (Termómetro de Eficacia/Eficiencia) + Bloque 4 (Penalización por Sesgos)
    // + Índice de Salud Decisional + Diccionario_Dimensiones.
    // Clase puramente C# (sin UnityEngine) para poder testearla fuera del motor de Unity.
    public static class MotorPuntuacion
    {
        // Umbral para convertir % Eficacia / % Eficiencia en los booleanos esEficaz / esEficiente
        // que consume puente_de_memoria.txt. No está definido en los documentos fuente:
        // es un valor por defecto razonable — AJUSTAR si el diseño define otro criterio.
        public const float UMBRAL_EFICAZ_EFICIENTE = 60f;

        // %Eficacia = Puntaje Obtenido / Puntaje Máximo × 100 (ídem %Eficiencia), según la
        // hoja Índice_Salud_Decisional. "Puntaje Obtenido" = Σ (respuesta normalizada 0-1 × peso).
        public static ResultadoTermometro CalcularTermometro(
            List<PreguntaEje> banco,
            List<Respuesta> respuestas,
            float umbral = UMBRAL_EFICAZ_EFICIENTE)
        {
            float puntajeEficacia = 0f, maxEficacia = 0f;
            float puntajeEficiencia = 0f, maxEficiencia = 0f;

            foreach (var respuesta in respuestas)
            {
                var pregunta = banco.FirstOrDefault(p => p.ID_Pregunta == respuesta.ID_Pregunta);
                if (pregunta == null) continue; // pregunta no encontrada en el banco activo

                float normalizado = Normalizar(pregunta, respuesta);

                puntajeEficacia += normalizado * pregunta.Peso_Eficacia;
                maxEficacia += pregunta.Peso_Eficacia;

                puntajeEficiencia += normalizado * pregunta.Peso_Eficiencia;
                maxEficiencia += pregunta.Peso_Eficiencia;
            }

            float pctEficacia = maxEficacia > 0f ? (puntajeEficacia / maxEficacia) * 100f : 0f;
            float pctEficiencia = maxEficiencia > 0f ? (puntajeEficiencia / maxEficiencia) * 100f : 0f;

            return new ResultadoTermometro
            {
                PorcentajeEficacia = pctEficacia,
                PorcentajeEficiencia = pctEficiencia,
                EsEficaz = pctEficacia >= umbral,
                EsEficiente = pctEficiencia >= umbral
            };
        }

        // Convierte la respuesta cruda en un valor 0-1.
        // Escala 1-5: (valor-1)/4, siempre en positivo (los bancos de eje redactan 5 = mejor).
        // Sí/No: se invierte si la pregunta tiene Polaridad "Negativa" (ver Modelos.cs).
        private static float Normalizar(PreguntaEje pregunta, Respuesta respuesta)
        {
            if (!respuesta.EsSiNo)
                return (respuesta.ValorEscala - 1) / 4f;

            bool esPositiva = pregunta.Polaridad != "Negativa";
            bool respuestaFavorable = esPositiva ? respuesta.ValorSiNo : !respuesta.ValorSiNo;
            return respuestaFavorable ? 1f : 0f;
        }

        // "Un 'Sí' a un comportamiento sesgado suma un valor fijo. Respuestas de escala 4 o 5
        // suman de manera proporcional al riesgo" — hoja Modificadores_Sesgos / puente_de_memoria.
        public static ResultadoSesgos CalcularPenalizacionSesgos(List<PreguntaSesgo> banco, List<Respuesta> respuestas)
        {
            float penalizacion = 0f;
            var activados = new List<string>();

            foreach (var respuesta in respuestas)
            {
                var pregunta = banco.FirstOrDefault(p => p.ID_Pregunta == respuesta.ID_Pregunta);
                if (pregunta == null) continue;

                float aporte = 0f;
                if (respuesta.EsSiNo)
                {
                    if (respuesta.ValorSiNo) aporte = pregunta.Penalizacion_Si;
                }
                else
                {
                    if (respuesta.ValorEscala == 4) aporte = pregunta.Penalizacion_Nivel4;
                    else if (respuesta.ValorEscala == 5) aporte = pregunta.Penalizacion_Nivel5;
                }

                if (aporte > 0f)
                {
                    penalizacion += aporte;
                    activados.Add(pregunta.Efecto_Diagnostico_IA);
                }
            }

            // Se limita a [0,1]: se usa como (1 - penalización) en el Índice de Salud Decisional
            // y no debería poder volverlo negativo.
            if (penalizacion < 0f) penalizacion = 0f;
            if (penalizacion > 1f) penalizacion = 1f;

            return new ResultadoSesgos { PenalizacionAcumulada = penalizacion, SesgosActivados = activados };
        }

        // Índice de Salud Decisional = [(%Eficacia + %Eficiencia) / 2] × (1 − Penalización por sesgos)
        // Fórmula tomada literalmente de la hoja Índice_Salud_Decisional / puente_de_memoria.txt.
        public static float CalcularIndiceSaludDecisional(ResultadoTermometro termometro, ResultadoSesgos sesgos)
        {
            float promedio = (termometro.PorcentajeEficacia + termometro.PorcentajeEficiencia) / 2f;
            return promedio * (1f - sesgos.PenalizacionAcumulada);
        }

        // Diccionario_Dimensiones: "Si 2 o más preguntas de un mismo tipo son afirmativas,
        // la decisión se etiqueta con ese tipo." Una decisión puede recibir más de un tipo.
        // 'respuestas' debe usar Codigo_Tipo (ej. "EST-01") como ID_Pregunta.
        public static List<TipoDecision> ClasificarTipoDecision(List<PreguntaDimension> banco, List<Respuesta> respuestas)
        {
            var conteos = new Dictionary<string, int>();

            foreach (var respuesta in respuestas)
            {
                if (!respuesta.EsSiNo || !respuesta.ValorSiNo) continue; // solo cuentan los "Sí"

                var pregunta = banco.FirstOrDefault(p => p.Codigo_Tipo == respuesta.ID_Pregunta);
                if (pregunta == null) continue;

                if (!conteos.ContainsKey(pregunta.Tipo_Decision)) conteos[pregunta.Tipo_Decision] = 0;
                conteos[pregunta.Tipo_Decision]++;
            }

            var tipos = new List<TipoDecision>();
            foreach (var kvp in conteos)
            {
                if (kvp.Value >= 2 && TryNormalizarTipo(kvp.Key, out var tipo))
                    tipos.Add(tipo);
            }
            return tipos;
        }

        private static bool TryNormalizarTipo(string tipoDecisionCrudo, out TipoDecision tipo)
        {
            switch (tipoDecisionCrudo)
            {
                case "ESTRATÉGICA": tipo = TipoDecision.Estrategica; return true;
                case "OPERATIVA": tipo = TipoDecision.Operativa; return true;
                case "CREATIVA": tipo = TipoDecision.Creativa; return true;
                default: tipo = default; return false;
            }
        }
    }
}

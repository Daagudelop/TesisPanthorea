using System.Collections.Generic;
using UnityEngine;
using JuegoSerio.Modelos;

namespace JuegoSerio.Datos
{
    // Carga los bancos de preguntas desde los .json ubicados en Assets/JuegoSerio/Resources/.
    // Unity solo indexa con Resources.Load() lo que está dentro de una carpeta llamada
    // exactamente "Resources" — de ahí la ubicación de los 3 archivos entregados.
    public static class CargadorBancoPreguntas
    {
        public static BancoPreguntasEje CargarBancoEjes(string nombreArchivo = "banco_preguntas")
        {
            TextAsset json = Resources.Load<TextAsset>(nombreArchivo);
            if (json == null)
            {
                Debug.LogError($"[JuegoSerio] No se encontró '{nombreArchivo}.json' en una carpeta Resources.");
                return new BancoPreguntasEje { preguntas = new List<PreguntaEje>() };
            }
            return JsonUtility.FromJson<BancoPreguntasEje>(json.text);
        }

        public static BancoPreguntasSesgos CargarBancoSesgos(string nombreArchivo = "banco_sesgos")
        {
            TextAsset json = Resources.Load<TextAsset>(nombreArchivo);
            if (json == null)
            {
                Debug.LogError($"[JuegoSerio] No se encontró '{nombreArchivo}.json' en una carpeta Resources.");
                return new BancoPreguntasSesgos { preguntas = new List<PreguntaSesgo>() };
            }
            return JsonUtility.FromJson<BancoPreguntasSesgos>(json.text);
        }

        public static BancoPreguntasDimensiones CargarBancoDimensiones(string nombreArchivo = "banco_dimensiones")
        {
            TextAsset json = Resources.Load<TextAsset>(nombreArchivo);
            if (json == null)
            {
                Debug.LogError($"[JuegoSerio] No se encontró '{nombreArchivo}.json' en una carpeta Resources.");
                return new BancoPreguntasDimensiones { preguntas = new List<PreguntaDimension>() };
            }
            return JsonUtility.FromJson<BancoPreguntasDimensiones>(json.text);
        }
    }
}

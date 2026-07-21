using System;
using System.Collections.Generic;

namespace JuegoSerio.Modelos
{
    public enum TipoDecision { Estrategica, Operativa, Creativa }

    public enum CuadranteEisenhower
    {
        UrgenteImportante,       // A
        NoUrgenteImportante,     // B
        UrgenteNoImportante,     // C
        NoUrgenteNoImportante,   // D
        NoAplica                 // Caso 1: eficaz y eficiente, no entra a la matriz
    }

    public enum PrioridadTransicion { Ninguna, Recomendada, Alta }

    // ---- Bancos de preguntas (se cargan desde los JSON en Resources/) ----

    [Serializable]
    public class PreguntaEje
    {
        public string ID_Pregunta;
        public string Eje_Tematico;   // "Interna" | "Relacional" | "Adaptativa"
        public string Tipo_Pregunta;  // "SiNo" | "Escala1a5"
        public string Criterio_Asociado;
        public string Texto_Pregunta;
        public float Peso_Eficacia;
        public float Peso_Eficiencia;
        // Solo aplica a preguntas "SiNo". "Positiva" = un Sí suma como respuesta favorable.
        // "Negativa" = un Sí se invierte (cuenta como desfavorable). Ver README_MotorCalculo.md:
        // esta columna NO existe en el Excel original, es una inferencia mía sobre la redacción
        // de cada pregunta — revísala antes de darla por definitiva.
        public string Polaridad;
    }

    [Serializable]
    public class BancoPreguntasEje
    {
        public List<PreguntaEje> preguntas;
    }

    [Serializable]
    public class PreguntaSesgo
    {
        public string ID_Pregunta;
        public string Sesgo_Nidificado;
        public string Tipo_Pregunta; // "SiNo" | "Escala1a5"
        public string Texto_Pregunta;
        public string Taxonomia_Afectada;
        public float Penalizacion_Si;
        public float Penalizacion_Nivel4;
        public float Penalizacion_Nivel5;
        public string Efecto_Diagnostico_IA;
    }

    [Serializable]
    public class BancoPreguntasSesgos
    {
        public List<PreguntaSesgo> preguntas;
    }

    [Serializable]
    public class PreguntaDimension
    {
        public string Codigo_Tipo;     // EST-01, OPE-03, CRE-02, ...
        public string Tipo_Decision;   // "ESTRATÉGICA" | "OPERATIVA" | "CREATIVA"
        public string Dimension_Enfoque;
        public string Pregunta_Guia;
    }

    [Serializable]
    public class BancoPreguntasDimensiones
    {
        public List<PreguntaDimension> preguntas;
    }

    // ---- Respuestas del jugador ----

    [Serializable]
    public class Respuesta
    {
        public string ID_Pregunta;
        public bool EsSiNo;     // true = respuesta booleana; false = respuesta de escala 1-5
        public bool ValorSiNo;  // usado si EsSiNo == true
        public int ValorEscala; // 1-5, usado si EsSiNo == false

        public static Respuesta CrearSiNo(string id, bool valor) =>
            new Respuesta { ID_Pregunta = id, EsSiNo = true, ValorSiNo = valor };

        public static Respuesta CrearEscala(string id, int valor)
        {
            if (valor < 1 || valor > 5)
                throw new ArgumentOutOfRangeException(nameof(valor), "La escala debe estar entre 1 y 5.");
            return new Respuesta { ID_Pregunta = id, EsSiNo = false, ValorEscala = valor };
        }
    }

    // Nodo de decisión — Bloque 1 (Registro Inicial e Identificación) del flujo.
    // Respeta las 3 propiedades exigidas en el diagrama: ID_Proyecto, Nombre_Decision, Contexto_Problema.
    [Serializable]
    public class DecisionRegistro
    {
        public string ID_Proyecto;
        public string Nombre_Decision;
        public string Contexto_Problema;

        public List<Respuesta> RespuestasEjes = new List<Respuesta>();
        public List<Respuesta> RespuestasSesgos = new List<Respuesta>();
        public List<Respuesta> RespuestasDimensiones = new List<Respuesta>(); // siempre Sí/No, ID = Codigo_Tipo
    }

    // ---- Resultados ----

    // Bloque 2: Termómetro de Eficacia y Eficiencia.
    [Serializable]
    public class ResultadoTermometro
    {
        public float PorcentajeEficacia;
        public float PorcentajeEficiencia;
        public bool EsEficaz;
        public bool EsEficiente;
    }

    // Bloque 4: Penalización por Sesgos Cognitivos.
    [Serializable]
    public class ResultadoSesgos
    {
        public float PenalizacionAcumulada; // 0-1
        public List<string> SesgosActivados = new List<string>(); // Efecto_Diagnostico_IA de cada sesgo disparado
    }

    // Paquete final: lo que alimenta la Pantalla FINAL y, más adelante, el prompt
    // hacia el LLM local (ollama-unity) para redactar el texto explicativo.
    [Serializable]
    public class ResultadoDecisionCompleto
    {
        public ResultadoTermometro Termometro;
        public ResultadoSesgos Sesgos;
        public float IndiceSaludDecisional;
        public CuadranteEisenhower Cuadrante;
        public PrioridadTransicion Prioridad;
        public bool PasarSiguienteFase;
        public List<TipoDecision> TiposDetectados = new List<TipoDecision>();
    }
}

#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using JuegoSerio.Juego;
using JuegoSerio.IA;
using JuegoSerio.UI;

namespace JuegoSerio.EditorTools
{
    // Construye y guarda una escena de demo COMPLETA con Canvas, GameManager y las 5 pantallas
    // ya conectadas entre sí. No es una escena hecha a mano en YAML (demasiado frágil de
    // escribir a ciegas): este script arma todo con la API real de Unity, así que si algo
    // fuera a fallar, fallaría como un error de compilación/consola visible, no como una
    // escena silenciosamente rota.
    //
    // USO: Unity > menú "JuegoSerio > Construir Escena Demo".
    // Requisitos: que el proyecto compile sin errores (incluye las 2 líneas de ollama-unity
    // marcadas como "verificar" en entregas anteriores) y que TMP Essential Resources ya esté
    // importado (Window > TextMeshPro > Import TMP Essential Resources).
    //
    // El estilo visual es deliberadamente mínimo (rectángulos de color plano) — es un
    // esqueleto funcional para verificar el flujo, no la propuesta de arte final.
    public static class ConstructorEscenaDemo
    {
        [MenuItem("JuegoSerio/Construir Escena Demo")]
        public static void Construir()
        {
            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CrearEventSystem();
            var canvas = CrearCanvas();

            // ---- GameManager + IA ----
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>(); // [RequireComponent] agrega OllamaReporteFinalGenerador solo
            gmGO.AddComponent<FacilitadorConversacional>(); // presente por si luego cambias al chat del Bloque 1
            gmGO.AddComponent<OllamaGeneradorPreguntas>(); // presente por si activas "generarPreguntasConIA" en GameManager

            // ---- Pantallas ----
            var panelRegistro = CrearPanelRegistro(canvas.transform);
            var panelPreguntas = CrearPanelPreguntas(canvas.transform);
            var panelCalculando = CrearPanelCalculando(canvas.transform);
            var panelFinal = CrearPanelFinal(canvas.transform);
            var panelFinDeSesion = CrearPanelFinDeSesion(canvas.transform);

            // Solo el panel de registro visible al abrir la escena (cosmético; en Play,
            // ControladorPantallas reajusta esto de inmediato según GameManager.Estado).
            panelPreguntas.SetActive(false);
            panelCalculando.SetActive(false);
            panelFinal.SetActive(false);
            panelFinDeSesion.SetActive(false);

            // ---- Controlador ----
            var controladorGO = new GameObject("ControladorPantallas");
            var controlador = controladorGO.AddComponent<ControladorPantallas>();
            AsignarCampoPrivado(controlador, "panelRegistro", panelRegistro);
            AsignarCampoPrivado(controlador, "panelPreguntas", panelPreguntas);
            AsignarCampoPrivado(controlador, "panelCalculando", panelCalculando);
            AsignarCampoPrivado(controlador, "panelFinal", panelFinal);
            AsignarCampoPrivado(controlador, "panelFinDeSesion", panelFinDeSesion);
            AsignarCampoPrivado(controlador, "pantallaPreguntas", panelPreguntas.GetComponent<PantallaPreguntasUI>());
            AsignarCampoPrivado(controlador, "pantallaFinal", panelFinal.GetComponent<PantallaFinalUI>());

            const string carpeta = "Assets/JuegoSerio/Scenes";
            if (!AssetDatabase.IsValidFolder(carpeta))
                AssetDatabase.CreateFolder("Assets/JuegoSerio", "Scenes");

            EditorSceneManager.SaveScene(escena, $"{carpeta}/JuegoSerioDemo.unity");
            Debug.Log("[JuegoSerio] Escena creada y guardada en Assets/JuegoSerio/Scenes/JuegoSerioDemo.unity");
        }

        // ================= Infraestructura básica =================

        private static void CrearEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;

            // Elige el módulo de input correcto según lo que el proyecto tenga activo en
            // Player Settings (Active Input Handling), en vez de asumir siempre el clásico.
#if ENABLE_INPUT_SYSTEM
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
        }

        private static GameObject CrearCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            return go;
        }

        // Panel raíz de pantalla: ocupa ~85% del canvas, con layout vertical automático.
        private static GameObject CrearPanelRaiz(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.08f);
            rt.anchorMax = new Vector2(0.92f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.09f, 0.92f);

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.padding = new RectOffset(32, 32, 32, 32);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            return go;
        }

        // Sub-grupo horizontal (fila de botones), pensado para vivir dentro de un panel raíz.
        private static GameObject CrearFilaHorizontal(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 50;
            return go;
        }

        private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, int tamano = 24,
            TextAlignmentOptions alineacion = TextAlignmentOptions.Center)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = contenido;
            tmp.fontSize = tamano;
            tmp.alignment = alineacion;
            tmp.color = Color.white;
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = tamano * 1.6f;
            return tmp;
        }

        private static Button CrearBoton(Transform padre, string nombre, string etiqueta, float ancho = 200f)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
            go.GetComponent<Image>().color = new Color(0.20f, 0.42f, 0.82f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 46;
            le.preferredWidth = ancho;

            var textoGO = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
            textoGO.transform.SetParent(go.transform, false);
            var rt = textoGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var tmp = textoGO.GetComponent<TextMeshProUGUI>();
            tmp.text = etiqueta;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 20;

            return go.GetComponent<Button>();
        }

        // TMP_InputField mínimo funcional: Text Area (con RectMask2D) + Placeholder + Text.
        private static TMP_InputField CrearCampoTexto(Transform padre, string nombre, string placeholder)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(padre, false);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 42;

            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(go.transform, false);
            var textAreaRT = textArea.GetComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero; textAreaRT.anchorMax = Vector2.one;
            textAreaRT.offsetMin = new Vector2(10, 6); textAreaRT.offsetMax = new Vector2(-10, -6);

            var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderGO.transform.SetParent(textArea.transform, false);
            EstirarCompleto(placeholderGO.GetComponent<RectTransform>());
            var placeholderTMP = placeholderGO.GetComponent<TextMeshProUGUI>();
            placeholderTMP.text = placeholder;
            placeholderTMP.fontStyle = FontStyles.Italic;
            placeholderTMP.color = new Color(1f, 1f, 1f, 0.5f);

            var textoGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textoGO.transform.SetParent(textArea.transform, false);
            EstirarCompleto(textoGO.GetComponent<RectTransform>());
            var textoTMP = textoGO.GetComponent<TextMeshProUGUI>();
            textoTMP.color = Color.white;

            var campo = go.GetComponent<TMP_InputField>();
            campo.textViewport = textAreaRT;
            campo.textComponent = textoTMP;
            campo.placeholder = placeholderTMP;
            campo.text = string.Empty;

            return campo;
        }

        private static void EstirarCompleto(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        // Asigna un valor a un campo privado [SerializeField] sin necesitar que sea público.
        private static void AsignarCampoPrivado(UnityEngine.Object objetivo, string nombreCampo, UnityEngine.Object valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(nombreCampo);
            if (prop == null)
            {
                Debug.LogWarning($"[JuegoSerio] No se encontró el campo '{nombreCampo}' en {objetivo.GetType().Name}.");
                return;
            }
            prop.objectReferenceValue = valor;
            so.ApplyModifiedProperties();
        }

        // Conecta un botón a un método público SIN parámetros, de forma persistente
        // (equivalente a arrastrarlo a mano en el Inspector).
        private static void ConectarBoton(Button boton, UnityEngine.Object objetivo, string nombreMetodo)
        {
            var metodo = objetivo.GetType().GetMethod(nombreMetodo, BindingFlags.Public | BindingFlags.Instance);
            var accion = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), objetivo, metodo);
            UnityEventTools.AddPersistentListener(boton.onClick, accion);
        }

        // ================= Pantallas =================

        private static GameObject CrearPanelRegistro(Transform canvas)
        {
            var panel = CrearPanelRaiz(canvas, "PanelRegistro");
            CrearTexto(panel.transform, "Titulo", "Registrar Decisión", 30);
            var campoId = CrearCampoTexto(panel.transform, "Input_IdProyecto", "ID del proyecto (ej. PROY-01)");
            var campoNombre = CrearCampoTexto(panel.transform, "Input_Nombre", "Nombre de la decisión");
            var campoContexto = CrearCampoTexto(panel.transform, "Input_Contexto", "Contexto / problema que la originó");
            var boton = CrearBoton(panel.transform, "Boton_Registrar", "Registrar decisión", 260f);

            var script = panel.AddComponent<PantallaRegistroUI>();
            AsignarCampoPrivado(script, "campoIdProyecto", campoId);
            AsignarCampoPrivado(script, "campoNombreDecision", campoNombre);
            AsignarCampoPrivado(script, "campoContexto", campoContexto);
            ConectarBoton(boton, script, nameof(PantallaRegistroUI.OnClickRegistrarDecision));

            return panel;
        }

        private static GameObject CrearPanelPreguntas(Transform canvas)
        {
            var panel = CrearPanelRaiz(canvas, "PanelPreguntas");
            var progreso = CrearTexto(panel.transform, "Texto_Progreso", "1/8", 18);
            var pregunta = CrearTexto(panel.transform, "Texto_Pregunta", "(pregunta)", 26);

            var filaEscala = CrearFilaHorizontal(panel.transform, "PanelEscala");
            var botonesEscala = new Button[5];
            for (int i = 1; i <= 5; i++)
                botonesEscala[i - 1] = CrearBoton(filaEscala.transform, $"Boton_{i}", i.ToString(), 60f);

            var filaSiNo = CrearFilaHorizontal(panel.transform, "PanelSiNo");
            var botonSi = CrearBoton(filaSiNo.transform, "Boton_Si", "Sí", 120f);
            var botonNo = CrearBoton(filaSiNo.transform, "Boton_No", "No", 120f);

            var script = panel.AddComponent<PantallaPreguntasUI>();
            AsignarCampoPrivado(script, "textoPregunta", pregunta);
            AsignarCampoPrivado(script, "textoProgreso", progreso);
            AsignarCampoPrivado(script, "panelEscala", filaEscala);
            AsignarCampoPrivado(script, "panelSiNo", filaSiNo);

            for (int i = 0; i < 5; i++)
            {
                var metodo = typeof(PantallaPreguntasUI).GetMethod(nameof(PantallaPreguntasUI.OnResponderEscala));
                var accion = (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>), script, metodo);
                UnityEventTools.AddIntPersistentListener(botonesEscala[i].onClick, accion, i + 1);
            }

            var metodoSiNo = typeof(PantallaPreguntasUI).GetMethod(nameof(PantallaPreguntasUI.OnResponderSiNo));
            var accionSiNo = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), script, metodoSiNo);
            UnityEventTools.AddBoolPersistentListener(botonSi.onClick, accionSiNo, true);
            UnityEventTools.AddBoolPersistentListener(botonNo.onClick, accionSiNo, false);

            return panel;
        }

        private static GameObject CrearPanelCalculando(Transform canvas)
        {
            var panel = CrearPanelRaiz(canvas, "PanelCalculando");
            var texto = CrearTexto(panel.transform, "Texto_Calculando", "Calculando diagnóstico", 28);
            var script = panel.AddComponent<PantallaCalculandoUI>();
            AsignarCampoPrivado(script, "texto", texto);
            return panel;
        }

        private static GameObject CrearPanelFinal(Transform canvas)
        {
            var panel = CrearPanelRaiz(canvas, "PanelFinal");
            CrearTexto(panel.transform, "Titulo", "Pantalla Final", 30);
            var eficacia = CrearTexto(panel.transform, "Texto_EficaciaEficiencia", "Eficacia: --%   Eficiencia: --%", 20);
            var indice = CrearTexto(panel.transform, "Texto_IndiceSalud", "Índice de Salud Decisional: --", 20);
            var cuadrante = CrearTexto(panel.transform, "Texto_CuadrantePrioridad", "Cuadrante: --   Prioridad: --", 18);
            var codigo = CrearTexto(panel.transform, "Texto_CodigoTecnico", "Código Técnico: --", 16);
            var resultado = CrearTexto(panel.transform, "Texto_ResultadoFinal", "(resultado final)", 24);
            var cta = CrearTexto(panel.transform, "Texto_CTA", "(CTA)", 20);

            var fila = CrearFilaHorizontal(panel.transform, "FilaBotones");
            var botonOtra = CrearBoton(fila.transform, "Boton_OtraDecision", "Registrar otra decisión", 260f);
            var botonTerminar = CrearBoton(fila.transform, "Boton_TerminarSesion", "Terminar sesión", 200f);

            var script = panel.AddComponent<PantallaFinalUI>();
            AsignarCampoPrivado(script, "textoEficaciaEficiencia", eficacia);
            AsignarCampoPrivado(script, "textoIndiceSalud", indice);
            AsignarCampoPrivado(script, "textoCuadrantePrioridad", cuadrante);
            AsignarCampoPrivado(script, "textoCodigoTecnico", codigo);
            AsignarCampoPrivado(script, "textoResultadoFinal", resultado);
            AsignarCampoPrivado(script, "textoCTA", cta);

            ConectarBoton(botonOtra, script, nameof(PantallaFinalUI.OnClickRegistrarOtraDecision));
            ConectarBoton(botonTerminar, script, nameof(PantallaFinalUI.OnClickTerminarSesion));

            return panel;
        }

        private static GameObject CrearPanelFinDeSesion(Transform canvas)
        {
            var panel = CrearPanelRaiz(canvas, "PanelFinDeSesion");
            CrearTexto(panel.transform, "Texto", "Gracias por jugar — sesión finalizada.", 26);
            return panel;
        }
    }
}
#endif

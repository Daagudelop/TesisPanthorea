# Pantallas de UI (uGUI + TextMeshPro)

Cierra el ciclo: son las pantallas que consumen los "ganchos" del `GameManager` (eventos +
métodos públicos) de las 3 entregas anteriores. Usé **uGUI clásico (Canvas) + TextMeshPro**
por ser lo más estándar — si tu equipo ya trabaja con UI Toolkit, avísame y adapto los mismos
scripts (la lógica de `ControladorPantallas`/`PantallaPreguntasUI` no cambiaría, solo los tipos
de UI usados).

## Requisito único

Si nunca usaste TextMeshPro en este proyecto: **Window → TextMeshPro → Import TMP Essential
Resources** (un solo click, Unity lo pide automáticamente la primera vez que arrastras un
componente TMP a una escena).

## Archivos nuevos

```
Assets/JuegoSerio/Scripts/UI/
├── ControladorPantallas.cs       # router: muestra/oculta paneles según GameManager.Estado
├── PantallaRegistroUI.cs         # Bloque 1 — formulario clásico (3 campos)
├── PantallaChatFacilitadorUI.cs  # Bloque 1 — alternativa conversacional (usa el facilitador)
├── PantallaPreguntasUI.cs        # reutilizable: Termómetro + Sesgos + Dimensiones
├── PantallaCalculandoUI.cs       # pantalla de espera (motor + LLM local)
└── PantallaFinalUI.cs            # resultado, Código Técnico, texto del LLM, CTA
```

## Jerarquía sugerida en el Canvas

```
Canvas
└── ControladorPantallas (script aquí)
    ├── PanelRegistro            (PantallaRegistroUI  o  PantallaChatFacilitadorUI — no ambos)
    │   ├── Input_IdProyecto (TMP_InputField)
    │   ├── Input_Nombre (TMP_InputField)
    │   ├── Input_Contexto (TMP_InputField)
    │   └── Boton_Registrar → OnClickRegistrarDecision()
    ├── PanelPreguntas            (PantallaPreguntasUI)
    │   ├── Texto_Pregunta (TMP_Text)
    │   ├── Texto_Progreso (TMP_Text)
    │   ├── PanelEscala
    │   │   └── Boton_1 .. Boton_5 → OnResponderEscala(1..5)
    │   └── PanelSiNo
    │       ├── Boton_Si → OnResponderSiNo(true)
    │       └── Boton_No → OnResponderSiNo(false)
    ├── PanelCalculando           (PantallaCalculandoUI)
    │   └── Texto_Calculando (TMP_Text)
    ├── PanelFinal                (PantallaFinalUI)
    │   ├── Texto_EficaciaEficiencia / Texto_IndiceSalud / Texto_CuadrantePrioridad
    │   ├── Texto_CodigoTecnico / Texto_ResultadoFinal / Texto_CTA
    │   ├── Boton_OtraDecision → OnClickRegistrarOtraDecision()
    │   └── Boton_TerminarSesion → OnClickTerminarSesion()
    └── PanelFinDeSesion          (un simple "Gracias por jugar")
```

Arrastra cada `Panel...` y su script correspondiente a los campos de `ControladorPantallas` en
el Inspector. **Deja los 6 paneles activos en el Editor mientras cableas los botones** —
`ControladorPantallas` se encarga de apagarlos/prenderlos en cuanto le des Play.

## Formulario vs. chat para el Bloque 1

`PantallaRegistroUI` y `PantallaChatFacilitadorUI` son intercambiables: usa la que prefieras
como contenido de `PanelRegistro`. Si usas la de chat, recuerda asignar el mismo
`FacilitadorConversacional` tanto ahí como en el campo `facilitador` del `GameManager`, para que
`IniciarNuevaDecision()` se dispare solo al terminar la charla.

## Botones de `PantallaPreguntasUI`

Los 5 botones de escala y los 2 de Sí/No son genéricos — sirven para las 3 preguntas cerradas
(Termómetro, Sesgos, Dimensiones) porque `PantallaPreguntasUI` decide con qué banco y qué
`GameManager.Registrar...()` conectarlos internamente. No necesitas duplicar el panel de
preguntas 3 veces.

## Prueba de punta a punta

1. Arma la jerarquía de arriba con datos placeholder en los textos.
2. Asegúrate de tener en la escena: `GameManager` + `OllamaReporteFinalGenerador` (+
   `FacilitadorConversacional` si usas el chat).
3. Dale Play y completa una decisión de principio a fin: deberías ver el formulario, luego las
   preguntas del Termómetro, luego Sesgos, luego Dimensiones, la pantalla de "Calculando...", y
   finalmente el resultado con el texto que redactó el LLM local (o el de respaldo si `ollama`
   no está corriendo).

Con esto las 5 piezas quedan completas: motor de cálculo, integración `ollama-unity`, código
técnico determinista, `GameManager`, facilitador conversacional y ahora las pantallas de UI.

# Escena Demo — cómo se arma

**No generé un archivo `.unity` a mano.** Escribir YAML de escena directamente es muy frágil —
un `fileID` mal referenciado deja componentes "perdidos" sin ningún error visible en Unity. En
vez de eso, escribí un **script de Editor** (`ConstructorEscenaDemo.cs`) que arma la escena
completa usando la API real de Unity: si algo estuviera mal, fallaría como un error de
compilación o de consola, no como una escena silenciosamente rota.

## Cómo generarla

1. Copia toda la carpeta `Assets/JuegoSerio` (con esta entrega ya son 4 subcarpetas: `Data` →
   ahora `Resources`, `Scripts`, `Scripts/IA`, `Scripts/Juego`, `Scripts/UI`, y `Editor`) a tu
   proyecto Unity.
2. Verifica que el proyecto **compile sin errores** — incluye confirmar las 2 líneas marcadas
   como "verificar" en `OllamaReporteFinalGenerador.cs` y `FacilitadorConversacional.cs` contra
   tu versión instalada de `ollama-unity`.
3. Asegúrate de tener **TMP Essential Resources** importado (Window → TextMeshPro → Import TMP
   Essential Resources), si no lo has hecho antes en este proyecto.
4. Menú **JuegoSerio → Construir Escena Demo**.
5. Se crea y guarda `Assets/JuegoSerio/Scenes/JuegoSerioDemo.unity`, ya abierta en el Editor.

## Qué arma exactamente

- Un `EventSystem` (con `StandaloneInputModule` — si tu proyecto usa el paquete Input System
  en modo exclusivo, cámbialo a mano por `InputSystemUIInputModule`).
- Un `Canvas` (Screen Space Overlay, `CanvasScaler` con resolución de referencia 1280×720).
- Un `GameObject "GameManager"` con `GameManager` + `OllamaReporteFinalGenerador` +
  `FacilitadorConversacional` (este último presente pero sin usar, por si luego cambias del
  formulario al chat del Bloque 1 — ver `README_FacilitadorConversacional.md`).
- Las 5 pantallas (`PanelRegistro`, `PanelPreguntas`, `PanelCalculando`, `PanelFinal`,
  `PanelFinDeSesion`) con sus scripts ya agregados y **todos los campos del Inspector ya
  asignados** (textos, botones, inputs) — no hay que arrastrar nada a mano.
- Los botones de escala (1-5) y Sí/No de `PanelPreguntas`, y los de `PanelFinal`, ya están
  conectados de forma persistente a los métodos correctos (lo mismo que arrastrarlos en el
  Inspector, hecho por código con `UnityEventTools`).
- Un `ControladorPantallas` con las 5 pantallas ya asignadas.

## Qué NO incluye (a propósito)

- **Estilo visual final.** Son rectángulos de color plano y texto — un esqueleto funcional para
  probar el flujo, no una propuesta de arte. El diseño visual (colores, tipografía, el Avatar
  Clippy, animaciones) queda pendiente para cuando el equipo de arte lo defina.
- **`PantallaChatFacilitadorUI`** en el Canvas — la escena usa el formulario clásico
  (`PantallaRegistroUI`) para el registro inicial, no el chat. Si prefieres el chat, es un
  cambio manual sencillo: quita `PantallaRegistroUI` de `PanelRegistro`, agrega
  `PantallaChatFacilitadorUI` con sus propios campos (historial + input + botón enviar), y
  asigna el `FacilitadorConversacional` del `GameManager` en el campo `facilitador` de
  `GameManager` y en `PantallaChatFacilitadorUI`.

## Probarla

1. Abre `Assets/JuegoSerio/Scenes/JuegoSerioDemo.unity` (o vuelve a correr el menú, es
   regenerable en cualquier momento — pisa el archivo anterior).
2. Dale Play.
3. Completa el formulario de registro, responde el Termómetro, los Sesgos y las Dimensiones, y
   deberías llegar a la Pantalla Final con el diagnóstico (del LLM local si `ollama serve` está
   corriendo, o el texto de respaldo si no).

Si algo no aparece o un botón no responde, lo más probable es un campo con el nombre distinto
al que el script del Editor esperaba — revisa la consola, `AsignarCampoPrivado` avisa con un
`Debug.LogWarning` si no encuentra el campo.

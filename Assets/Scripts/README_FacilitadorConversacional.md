# Facilitador Conversacional — Bloque 1

Cubre las dos frases del diagrama que quedaban pendientes:
*"Si el usuario no las tiene claras, la IA actúa como facilitadora con preguntas directas"*
y *"IA reorganiza la información registrada y realiza preguntas en caso de ser necesario"*.

## Archivos nuevos

```
Assets/JuegoSerio/Scripts/
├── IA/FacilitadorConversacional.cs        # conversación con InitChat()/Chat()
├── PruebaFacilitadorConversacional.cs     # demo: simula una charla completa
└── Juego/GameManager.cs                   # (modificado) campo opcional "facilitador"
```

## Por qué `Chat()` y no `ChatWithTool()`

`ollama-unity` también ofrece `ChatWithTool()` para function-calling, que en teoría sería más
directo para esto (el modelo "llamaría" a una función `registrar_decision(...)`). No lo usé
porque **no pude confirmar con certeza la firma exacta de esa función** en la documentación
pública disponible, y prefiero no construir sobre un supuesto no verificado para una pieza que
sí necesito que sea confiable.

En su lugar, uso el patrón más simple y bien documentado (`InitChat()` + `Chat()`, conversación
con memoria) y le pido al modelo, por prompt, que agregue una línea-marcador
(`REGISTRO_LISTO:{...}`) al final de su respuesta cuando ya reunió los dos datos que faltan
(`Nombre_Decision`, `Contexto_Problema`). Esa línea se parsea en C# con `JsonUtility`, nunca se
le muestra al jugador, y dispara el evento `OnDecisionExtraida`.

Si más adelante confirman la firma real de `ChatWithTool()`, se puede migrar sin tocar el resto
del sistema: solo cambiaría la implementación interna de `FacilitadorConversacional`, la interfaz
hacia `GameManager` (el evento `OnDecisionExtraida`) queda igual.

## ⚠️ Punto a verificar (igual que en la entrega anterior)

Asumí estas firmas para `InitChat()` y `Chat()`:

```csharp
await Ollama.InitChat(modelo, promptSistema);
string respuesta = await Ollama.Chat(modelo, mensajeUsuario);
```

Están aisladas en las 2 líneas marcadas dentro de `FacilitadorConversacional.cs`. Si el
compilador se queja del nombre/orden de parámetros, es lo único que hay que tocar.

## Cómo se conecta con el `GameManager`

`GameManager` ahora tiene un campo opcional `facilitador` (arrástralo en el Inspector si lo
usas). Si está asignado, se suscribe solo a `OnDecisionExtraida` y llama a
`IniciarNuevaDecision(...)` automáticamente — el jugador nunca ve un formulario, solo conversa.

Si **no** lo asignas, `GameManager` sigue funcionando exactamente igual que en la entrega
anterior: la UI llama a `IniciarNuevaDecision(idProyecto, nombre, contexto)` directamente con
un formulario tradicional. Las dos formas de registrar una decisión (formulario o conversación)
son intercambiables y no se pisan entre sí.

## Probarlo

1. Crea un GameObject con `FacilitadorConversacional` y otro (o el mismo) con `GameManager` +
   `OllamaReporteFinalGenerador`. Asigna el `FacilitadorConversacional` al campo `facilitador`
   del `GameManager` si quieres ver la conexión automática.
2. Agrega `PruebaFacilitadorConversacional` a la escena y dale Play.
3. La consola muestra 3 turnos simulados de un jugador que "no tiene clara" su decisión al
   principio, y al final el estado del `GameManager` debería mostrar `ResponderTermometro` si
   el modelo local logró extraer los datos correctamente.

Con esto quedan cubiertas las 4 piezas de IA/orquestación que planteamos al inicio. Lo único
que sigue sin construir son las pantallas de UI en sí (Canvas/UI Toolkit) — este entregable y
los anteriores solo dejan los "ganchos" (métodos públicos + eventos) para que cualquier UI se
enganche.

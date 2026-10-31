# TesisPanthorea

Juego serio (*serious game*) hecho en Unity para **registrar y evaluar decisiones**. El jugador describe una decisión que tomó, responde tres bloques de preguntas y el juego calcula qué tan **eficaz** y **eficiente** fue, cuánto la afectaron los **sesgos cognitivos** y en qué **cuadrante de la matriz de Eisenhower** debe priorizarse. Un asistente animado (Clippy) reacciona al resultado y un **LLM local (Ollama)** redacta el texto explicativo.

Todo el flujo corre en local: no necesita internet ni servicios de pago, solo Ollama instalado con un modelo descargado.

- **Motor:** Unity `6000.3.19f1`, URP 17.3, Input System, TextMeshPro.
- **IA local:** [ollama-unity](https://github.com/Haoming02/ollama-unity) (incluido en `Assets/ollama-unity`), modelo por defecto `gemma3:4b`.
- **Licencia:** MIT.

---

## Cómo funciona el juego

Cada decisión recorre este flujo (los estados están en `EstadoJuego`). Es el diagrama "Bitácora de Decisiones" que cita el código:

```
Registro inicial → (Generando preguntas) → Termómetro → Sesgos → Dimensiones
        → Calculando → Pantalla final → ¿otra decisión? → Fin de sesión
```

1. **Registro inicial.** Se identifica la decisión: `ID_Proyecto`, `Nombre_Decision` y `Contexto_Problema`. Hay dos formas de hacerlo:
   - **Formulario** de tres campos (`PantallaRegistroUI`).
   - **Chat con un facilitador de IA** (`PantallaChatFacilitadorUI` + `FacilitadorConversacional`). Si el jugador no tiene clara su decisión, el LLM le hace preguntas hasta reunir el nombre y el contexto, y registra la decisión solo.
2. **Termómetro de Eficacia y Eficiencia.** Preguntas de escala 1–5 y Sí/No, repartidas en tres ejes: *Interna*, *Relacional* y *Adaptativa*. Cada una pesa distinto en eficacia y en eficiencia. El banco puede ser fijo (JSON) o **generado por el LLM** a medida de la decisión; si la generación falla, se usa el fijo.
3. **Sesgos cognitivos.** Preguntas que detectan sesgos y restan puntos al resultado.
4. **Dimensiones.** Doce preguntas Sí/No que clasifican la decisión como *Estratégica*, *Operativa* y/o *Creativa*.
5. **Cálculo.** El motor en C# calcula todos los números y el LLM solo redacta el texto.
6. **Pantalla final.** Muestra los porcentajes, el índice, el cuadrante, la prioridad, el código técnico y el texto del LLM (resultado final + llamado a la acción). Clippy reacciona con una animación.
7. Cada decisión cerrada se guarda en la **Bitácora** de la sesión (`GameManager.Bitacora`).

---

## El motor de cálculo

Está en `Assets/Scripts/MotorPuntuacion.cs`, `MotorPriorizacion.cs` y `OrquestadorMotorDecision.cs`. Son clases estáticas de C# puro, sin dependencia de la escena.

### Termómetro
Cada respuesta se normaliza a un valor entre 0 y 1: en la escala 1–5 es `(valor − 1) / 4`, y en Sí/No es 1 o 0 según la **polaridad** de la pregunta (`Positiva` o `Negativa`). Luego:

```
%Eficacia   = Σ(respuesta × Peso_Eficacia)   / Σ Peso_Eficacia   × 100
%Eficiencia = Σ(respuesta × Peso_Eficiencia) / Σ Peso_Eficiencia × 100
```

Con un **umbral de 60 %** (`UMBRAL_EFICAZ_EFICIENTE`) se obtienen los booleanos `EsEficaz` y `EsEficiente`.

### Penalización por sesgos
Un "Sí" a un comportamiento sesgado suma una penalización fija. En las preguntas de escala solo cuentan las respuestas 4 y 5, con penalizaciones propias. El total se limita entre 0 y 1.

### Índice de Salud Decisional
```
Índice = ((%Eficacia + %Eficiencia) / 2) × (1 − Penalización por sesgos)
```

### Tipo de decisión
Si **2 o más** preguntas afirmativas pertenecen al mismo tipo (`EST-xx`, `OPE-xx`, `CRE-xx`), la decisión se etiqueta con ese tipo. Una decisión puede tener más de uno.

### Matriz de priorización ("puente de memoria")

| Eficaz | Eficiente | Resultado | Cuadrante | Prioridad |
|:-:|:-:|---|:-:|---|
| Sí | Sí | Éxito total, se archiva y no pasa de fase | — | Ninguna |
| No | Sí | Error de enfoque: reevaluar y simplificar | C (urgente, no importante) | Ninguna |
| No | No | Alerta crítica: pasa de inmediato a la fase táctica | A (urgente e importante) | Alta |
| Sí | No | Costosa pero sirvió: reevaluar recursos | B (no urgente, importante) | Recomendada |

### Código técnico
Cadena determinista con formato `EF{1|0}EI{1|0}-{A|B|C|D|N}` (por ejemplo `EF1EI0-B`). La calcula `CodigoTecnicoGenerador` y **nunca el LLM**. Sirve para elegir el frame de animación de Clippy.

---

## Bancos de preguntas

Están en `Assets/Scripts/Resources/` como JSON y se cargan con `CargadorBancoPreguntas`:

| Archivo | Preguntas | Contenido |
|---|:-:|---|
| `banco_preguntas.json` | 20 | Ejes Interna / Relacional / Adaptativa, con pesos de eficacia y eficiencia y polaridad. |
| `banco_sesgos.json` | 6 | Sesgos cognitivos, con penalización por "Sí" y por niveles 4 y 5, y el efecto diagnóstico que se pasa al LLM. |
| `banco_dimensiones.json` | 12 | Preguntas guía por tipo de decisión (`EST-`, `OPE-`, `CRE-`). |

Para cambiar o ampliar las preguntas basta con editar estos JSON; el motor no cambia.

---

## Integración con el LLM local (Ollama)

Todo está detrás de interfaces, así que se puede cambiar de proveedor sin tocar el resto del juego.

| Script | Función |
|---|---|
| `OllamaBootstrap` | Arranca el servidor de Ollama una sola vez por sesión y ofrece un *timeout* para las llamadas. |
| `OllamaReporteFinalGenerador` (`IReporteFinalGenerador`) | Redacta el *Resultado final* y el *CTA*. Se le pasan los números ya calculados y tiene prohibido recalcularlos. Si Ollama no responde, usa un texto de respaldo, así que la pantalla final nunca queda vacía. |
| `OllamaGeneradorPreguntas` (`IGeneradorPreguntas`) | Inventa el banco del Termómetro para cada decisión, con preguntas redactadas de forma que "Sí" siempre sea la respuesta favorable. Se activa con la casilla `generarPreguntasConIA` del `GameManager`. |
| `FacilitadorConversacional` | Chat que ayuda a definir la decisión y emite `OnDecisionExtraida` al reunir los datos. |

Las respuestas del modelo se piden en JSON y se extraen y validan en C# (tolera texto o *code fences* alrededor).

---

## Asistente Clippy (`Assets/ReactionMiniHelper`)

Avatar animado que reacciona al resultado del Termómetro.

- `AIResponseEvaluator` convierte (`esEficaz`, `esEficiente`) en una reacción y un diálogo: `Success`, `FocusError`, `Critical` o `ResourceWarning`.
- `HelperAgentController` encola y reproduce las reacciones, y `LocalAIResponseHandler` recibe el JSON del resultado.
- Incluye el prefab `AgentHelper` y animaciones como `IdleBase`, `FocusError` y `Critical`.

---

## Interfaz

- `ControladorPantallas`: único script que conoce todas las pantallas; activa el panel que toca según el estado del `GameManager`.
- Pantallas: `PantallaRegistroUI` o `PantallaChatFacilitadorUI`, `PantallaPreguntasUI` (un solo panel reutilizado para los tres bloques), `PantallaCalculandoUI` y `PantallaFinalUI`.
- La UI no toca el motor: solo llama a los métodos públicos de `GameManager` y escucha sus eventos (`OnEstadoCambiado`, `OnReporteListo`).
- `ConstructorEscenaDemo` es un script de editor que construye una **escena demo completa** (menú **JuegoSerio > Construir Escena Demo**). El estilo es mínimo, pensado para comprobar el flujo.

---

## Escenas

| Escena | Para qué sirve |
|---|---|
| `MODELOgeneradordePreguntas` | Versión del juego serio con preguntas generadas por IA (por su nombre y sus referencias a Ollama). |
| `modelNOAIGenerated` | Versión con el banco de preguntas fijo. |
| `SampleScene` y `Reigns/SampleScene` | Escenas de ejemplo y del prototipo Reigns. |

---

## Prototipo Reigns (`Assets/Scripts/Reigns`)

Prototipo de mecánica tipo *Reigns* (cartas con cuatro estadísticas):
- `Card`: carta con interacción de puntero.
- `StatManager`: elige una frase al azar y suma o resta valores a cuatro estadísticas al decidir.
- `MouseLookCharacter` y `MousePos`: el personaje 3D gira hacia el ratón y un objeto sigue el cursor.

No parece estar conectado al flujo principal del juego serio.

---

## Scripts de prueba

Sirven para verificar cada parte sin montar la UI. Se agregan a un GameObject vacío y se da Play:

- `PruebaMotorDecision`: motor de cálculo.
- `PruebaReporteFinal`: motor más LLM.
- `PruebaFlujoGameManager`: flujo completo.
- `PruebaFacilitadorConversacional`: conversación con el facilitador.

---

## Cómo ejecutarlo

1. Instala **Unity 6000.3.19f1** (o la versión de `ProjectSettings/ProjectVersion.txt`) y abre la carpeta del proyecto.
2. Instala [Ollama](https://ollama.com) y descarga el modelo:
   ```
   ollama pull gemma3:4b
   ```
   Si usas otro modelo, cámbialo en el campo `modelo` de los componentes `Ollama...` y `FacilitadorConversacional`.
3. Si la UI muestra textos en blanco, importa los recursos de TextMeshPro: **Window > TextMeshPro > Import TMP Essential Resources**.
4. Abre una de las escenas (por ejemplo `modelNOAIGenerated`) o crea la demo desde **JuegoSerio > Construir Escena Demo**, y pulsa Play.

Sin Ollama el juego funciona igual: el reporte final usa el texto de respaldo y el banco de preguntas fijo.

---

## Estructura

```
Assets/
├── Scripts/                 Motor, GameManager, IA, pantallas y pruebas
│   ├── Resources/           banco_preguntas / banco_sesgos / banco_dimensiones (.json)
│   └── Reigns/              Prototipo de cartas
├── ReactionMiniHelper/      Clippy: scripts, prefab y animaciones
├── ollama-unity/            Librería de terceros para hablar con Ollama
├── Scenes/                  Escenas del juego serio y del prototipo Reigns
├── Plugins/SerializedCollections/   Diccionarios serializables
├── Prefabs, Material, Animation, Settings, TextMesh Pro
Packages/ · ProjectSettings/
```

---

## Notas de diseño

Varias decisiones están marcadas en los comentarios del código como supuestos del autor, por si hay que ajustarlas:
- El umbral del 60 % para considerar una decisión eficaz o eficiente.
- La polaridad de las preguntas Sí/No del banco fijo.
- La correspondencia entre los cuatro casos eficaz/eficiente y los cuadrantes A/B/C/D.
- El formato del código técnico.

## Licencia

MIT. `ollama-unity` trae su propia licencia en `Assets/ollama-unity/LICENSE`.

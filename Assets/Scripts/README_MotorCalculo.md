# Motor de Cálculo — Bitácora de Decisiones (sin IA)

Implementa en C# puro los Bloques **2 (Termómetro de Eficacia/Eficiencia)**, **3 (Matriz de
Priorización / Eisenhower)** y **4 (Penalización por Sesgos Cognitivos)** del flujo, más el
**Diccionario_Dimensiones** y la lógica de `puente_de_memoria.txt`. Todo es determinista:
preguntas cerradas, pesos fijos del Excel, fórmulas fijas — no requiere el LLM local.
El LLM (vía `ollama-unity`) entra recién en la Pantalla FINAL, para redactar el texto
explicativo a partir de estos números; eso queda para la siguiente entrega.

## Estructura de archivos

```
Assets/JuegoSerio/
├── Resources/
│   ├── banco_preguntas.json      # 20 preguntas: Eje_Interna(8) + Eje_Relacional(6) + Eje_Adaptativa(6)
│   ├── banco_sesgos.json         # 6 preguntas de Modificadores_Sesgos
│   └── banco_dimensiones.json    # 12 preguntas del Diccionario_Dimensiones
└── Scripts/
    ├── Modelos.cs                 # clases de datos (preguntas, respuestas, resultados)
    ├── BancoPreguntas.cs          # carga los JSON con Resources.Load<TextAsset>
    ├── MotorPuntuacion.cs         # %Eficacia, %Eficiencia, sesgos, Índice de Salud, tipo de decisión
    ├── MotorPriorizacion.cs       # Matriz de Eisenhower + puente_de_memoria.txt
    ├── OrquestadorMotorDecision.cs# une los tres bloques en una sola llamada
    └── PruebaMotorDecision.cs     # demo MonoBehaviour, corre un caso completo y lo imprime
```

Copia la carpeta `Assets/JuegoSerio` completa dentro de tu proyecto Unity. Los `.json` **deben**
quedar dentro de una carpeta llamada literalmente `Resources` (ya vienen así) para que
`Resources.Load` los encuentre.

## Cómo probarlo

1. Arrastra `PruebaMotorDecision.cs` a cualquier GameObject vacío en una escena.
2. Dale Play. La consola imprime %Eficacia, %Eficiencia, penalización, Índice de Salud
   Decisional, cuadrante, prioridad y tipos de decisión detectados para un caso de ejemplo.
3. Con los datos de ejemplo incluidos en el script, el resultado esperado es:

   ```
   %Eficacia: 76.5 | %Eficiencia: 25.0
   Penalización por sesgos: 0.25
   Índice de Salud Decisional: 38.1
   Cuadrante: NoUrgenteImportante | Prioridad: Recomendada | ¿Pasa de fase?: True
   Tipos detectados: Estrategica
   ```

   Si te da esto, el motor está corriendo igual que en mi verificación en Python.

## Fórmulas (tomadas literalmente de los documentos fuente)

- **% Eficacia** = Σ(respuesta normalizada 0-1 × `Peso_Eficacia`) / Σ`Peso_Eficacia` × 100
- **% Eficiencia** = igual, con `Peso_Eficiencia`
- Escala 1-5 → normalizado = `(valor-1)/4`
- Sí/No → normalizado = 1 si la respuesta es favorable, 0 si no (ver "Polaridad" abajo)
- **Penalización por sesgos** = Σ (`Penalizacion_Si` si respondió "Sí" | `Penalizacion_Nivel4`/`_Nivel5`
  si escala 4 o 5), acotada a [0,1]
- **Índice de Salud Decisional** = [(%Eficacia + %Eficiencia) / 2] × (1 − Penalización)
- **Diccionario_Dimensiones**: si ≥2 preguntas "Sí" de un mismo tipo (EST/OPE/CRE), la decisión
  se etiqueta con ese tipo (puede tener varios tipos a la vez)

## ⚠️ 3 decisiones de diseño que tomé y que debes validar

Los documentos fuente no las especifican del todo; elegí la interpretación más razonable pero
**no son un hecho**, revísalas con el equipo antes de darlas por definitivas:

1. **Polaridad de las preguntas Sí/No de los ejes.** El Excel no dice si un "Sí" suma o resta
   — algunas preguntas están redactadas en positivo ("¿se ejecutó a tiempo?") y otras en
   negativo ("¿hubo miembros desmotivados?"). Añadí un campo `Polaridad` (`Positiva`/`Negativa`)
   a cada pregunta Sí/No en `banco_preguntas.json`, infiriéndolo de la redacción. La más dudosa
   es **`ADA_004`** ("¿el impacto real fue distinto al anticipado?"), donde asumí que un "Sí"
   es favorable (buena capacidad adaptativa); podría defenderse lo contrario. Es un campo de
   datos, no de código — se corrige directamente en el JSON sin tocar C#.

2. **Umbral esEficaz/esEficiente.** `puente_de_memoria.txt` usa dos booleanos, pero ningún
   documento dice qué % los activa. Usé **60%** como corte por defecto
   (`MotorPuntuacion.UMBRAL_EFICAZ_EFICIENTE`), parametrizable en cada llamada.

3. **Mapeo Caso 1-4 → Cuadrante A/B/C/D.** `puente_de_memoria.txt` da 4 casos pero no dice a
   qué cuadrante del Eisenhower corresponde cada uno; el diagrama da los 4 cuadrantes pero no
   los conecta con el código. Los uní así (documentado también en `MotorPriorizacion.cs`):

   | Caso (memoria) | Eficaz | Eficiente | Cuadrante | Prioridad |
   |---|---|---|---|---|
   | 1 | Sí | Sí | No aplica (se archiva) | Ninguna |
   | 2 | No | Sí | C — Urgente/No Importante | Ninguna |
   | 3 | No | No | A — Urgente e Importante | **Alta** |
   | 4 | Sí | No | B — No Urgente e Importante | **Recomendada** |

## Siguiente paso (no incluido aquí)

`ResultadoDecisionCompleto` queda listo para serializarse y pasarse como contexto al prompt de
`GenerateJson()` de `ollama-unity`, para que el LLM local redacte el "Resultado final" en texto
natural y el CTA de la Pantalla FINAL — sin que el modelo tenga que inventar ni recalcular
ningún número.

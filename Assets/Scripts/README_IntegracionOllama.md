# Integración ollama-unity — Pantalla FINAL

Conecta el motor de cálculo (entrega anterior) con un LLM local vía
[`ollama-unity`](https://github.com/Haoming02/ollama-unity) para redactar el **"Resultado
final"** y el **CTA** de la Pantalla FINAL usando `GenerateJson()`.

## Regla de diseño

**El modelo solo redacta, nunca calcula.** Todos los porcentajes, el cuadrante, la prioridad y
el `Código_Técnico` ya vienen resueltos en C# por el motor de cálculo; se le pasan al LLM como
datos fijos dentro del prompt, y se le pide únicamente que los convierta en 1-2 frases naturales
más un CTA. Esto evita que un modelo local pequeño (`gemma3:4b`, `gemma3:1b`, etc.) "alucine"
un número distinto al ya calculado.

El `Código_Técnico` (el que controla el frame del Avatar Clippy) tampoco se le pide al LLM —
se genera de forma determinista en `CodigoTecnicoGenerador.cs`, porque conduce lógica de juego
y necesita ser 100% reproducible.

## Archivos nuevos

```
Assets/JuegoSerio/Scripts/
├── Modelos_ReportePantallaFinal.cs   # clase ReportePantallaFinal { ResultadoFinal, CTA }
├── CodigoTecnicoGenerador.cs          # código alfanumérico determinista (no-LLM)
├── PruebaReporteFinal.cs              # demo: motor + LLM local encadenados
└── IA/
    ├── IReporteFinalGenerador.cs           # interfaz — aísla la dependencia de ollama-unity
    └── OllamaReporteFinalGenerador.cs      # implementación con Ollama.GenerateJson()
```

## Requisitos antes de compilar en Unity

1. **Newtonsoft.Json**: Window → Package Manager → Add package by name → `com.unity.nuget.newtonsoft-json`
2. Importa el `.unitypackage` de [ollama-unity Releases](https://github.com/Haoming02/ollama-unity/releases) (v4.0 al momento de escribir esto).
3. Ten `ollama serve` corriendo localmente y el modelo descargado:
   ```
   ollama pull gemma3:4b
   ```
4. Arrastra `OllamaReporteFinalGenerador.cs` y `PruebaReporteFinal.cs` al mismo GameObject
   vacío en una escena de prueba, dale Play.

## ⚠️ Punto a verificar (no lo pude confirmar 100%)

El README público de `ollama-unity` documenta `GenerateJson()` como *"Return the response in
the specified class/struct format"* pero **no publica la firma completa del método** (no tengo
acceso al código fuente exacto, solo a la documentación). Asumí:

```csharp
ReportePantallaFinal reporte = await Ollama.GenerateJson<ReportePantallaFinal>(modelo, prompt);
```

Está aislado en **una sola línea**, señalada con comentarios en `OllamaReporteFinalGenerador.cs`.
Si tu versión instalada tiene un nombre de parámetro distinto (o pide instanciar `Ollama` en vez
de llamarlo estático), el autocompletado de Visual Studio/Rider te lo va a corregir ahí mismo —
no hace falta tocar nada más del archivo, porque el resto del sistema (prompt, parsing,
respaldo) no depende de esos detalles.

## Comportamiento sin conexión / si ollama no responde

Si `ollama serve` no está corriendo, el modelo no existe localmente, o el JSON devuelto no
parsea, `GenerarAsync()` cae a un texto de respaldo determinista (`GenerarRespaldo()`) construido
directamente desde `EsEficaz`/`EsEficiente`/`PasarSiguienteFase`, para que la Pantalla FINAL
nunca se quede en blanco durante el desarrollo o una demo sin internet/GPU.

## Siguiente paso posible

El **facilitador conversacional** (Bloque 1: *"IA pregunta por la primera decisión a registrar"*,
*"IA reorganiza la información... y realiza preguntas en caso de ser necesario"*) usaría
`InitChat()` + `Chat()`/`ChatStream()` en vez de `GenerateJson()`, ya que necesita memoria de
turnos previos. Es una pieza separada — avísame cuando quieras que la arme.

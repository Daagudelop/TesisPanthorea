# Arquitectura Completa — GameManager

Une las dos entregas anteriores (Motor de Cálculo + integración `ollama-unity`) en un único
orquestador central, siguiendo el diagrama completo del flujo.

## Archivos nuevos

```
Assets/JuegoSerio/Scripts/
├── Juego/
│   ├── EstadoJuego.cs          # enum con los bloques del diagrama
│   ├── BitacoraEntrada.cs      # una fila de la "Bitácora de Decisiones"
│   └── GameManager.cs          # orquestador central (singleton)
└── PruebaFlujoGameManager.cs   # demo: simula una decisión completa sin UI
```

## Cómo se relaciona con lo ya entregado

`GameManager` **no reemplaza** nada de las dos entregas anteriores — las llama. La UI real
(Canvas, botones, sliders) solo debería hablarle a `GameManager`, nunca directamente a
`MotorPuntuacion`, `MotorPriorizacion` u `OllamaReporteFinalGenerador`:

```
UI  ──▶  GameManager  ──▶  MotorPuntuacion / MotorPriorizacion  (Bloques 2, 3, 4)
                      ──▶  OllamaReporteFinalGenerador          (Pantalla FINAL)
                      ──▶  CodigoTecnicoGenerador                (Avatar Clippy)
```

Esto deja toda la lógica de negocio en un solo lugar, sin acoplar el motor de cálculo ni la
llamada al LLM a ninguna pantalla específica — se puede cambiar el Canvas por UI Toolkit, o
`ollama-unity` por otro proveedor, sin tocar `GameManager`.

## Máquina de estados (`EstadoJuego`)

```
RegistroInicial ──▶ ResponderTermometro ──▶ ResponderSesgos ──▶ ResponderDimensiones
                                                                        │
                                                                        ▼
                                                                  Calculando
                                                                        │
                                                                        ▼
                                                                 PantallaFinal
                                                                   │        │
                                              RegistrarOtraDecision()   TerminarSesion()
                                                   │                        │
                                                   ▼                        ▼
                                          RegistroInicial               FinDeSesion
```

La UI se suscribe a `GameManager.OnEstadoCambiado` para saber qué pantalla mostrar, y a
`GameManager.OnReporteListo` para pintar el resultado (%Eficacia, %Eficiencia, Índice de Salud,
Código Técnico, texto del LLM y CTA) apenas están listos.

## Cómo lo usaría una pantalla de UI (ejemplo conceptual)

```csharp
// Pantalla de registro inicial
void OnClickContinuar()
{
    GameManager.Instancia.IniciarNuevaDecision(idProyectoInput.text, nombreInput.text, contextoInput.text);
}

// Pantalla del Termómetro — una PreguntaEje a la vez, recorriendo GameManager.Instancia.BancoEjes
void OnClickSiguientePregunta(int valorEscalaOSiNo)
{
    GameManager.Instancia.RegistrarRespuestaEje(Respuesta.CrearEscala(preguntaActual.ID_Pregunta, valorEscalaOSiNo));
    // si era la última pregunta del banco:
    GameManager.Instancia.TerminarTermometro();
}
```

## Probarlo

1. Crea un GameObject vacío llamado `GameManager` en tu escena.
2. Agrégale los componentes `GameManager` y `OllamaReporteFinalGenerador` (el `[RequireComponent]`
   ya fuerza esto).
3. Agrégale también `PruebaFlujoGameManager` para correr una decisión completa sin UI.
4. Dale Play. La consola debería mostrar la secuencia de estados
   (`RegistroInicial → ResponderTermometro → ResponderSesgos → ResponderDimensiones →
   Calculando → PantallaFinal`) y terminar con el mismo resultado verificado en las entregas
   anteriores (%Eficacia 76.5, %Eficiencia 25.0, Índice de Salud 38.1, Cuadrante B, Prioridad
   Recomendada).

## Lo que falta (pendiente, no incluido aquí)

- **Facilitador conversacional** (Bloque 1: *"IA pregunta por la primera decisión a registrar"*,
  *"IA reorganiza la información... y realiza preguntas en caso de ser necesario"*) — usaría
  `InitChat()` + `Chat()`/`ChatStream()` de ollama-unity, y probablemente reemplazaría/complementaría
  `IniciarNuevaDecision()` con una conversación de varios turnos en vez de 3 campos de texto fijos.
- Las pantallas de UI en sí (Canvas/UI Toolkit): este entregable deja los "ganchos" (eventos,
  métodos públicos) pero no dibuja nada en pantalla.

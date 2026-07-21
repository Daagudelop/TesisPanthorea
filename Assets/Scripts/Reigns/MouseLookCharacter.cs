using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hace que un personaje (modelo 3D) rote para "mirar" hacia el mouse,
/// igual que el efecto del inventario de Minecraft.
///
/// Uso típico:
/// - El personaje 3D se muestra dentro de la escena (o renderizado a un
///   RenderTexture que luego se muestra en un RawImage de la UI, que es
///   la forma más común de hacer un "preview" de inventario en 2D/UI).
/// - Este script va sobre el GameObject raíz del modelo (o sobre un
///   "pivot" vacío que sea el padre del modelo).
/// </summary>
public class MouseLookCharacter : MonoBehaviour
{
    [Header("Límites de rotación (en grados)")]
    [Tooltip("Rotación horizontal máxima hacia cada lado")]
    public float maxYaw = 40f;

    [Tooltip("Rotación vertical máxima hacia arriba/abajo")]
    public float maxPitch = 20f;

    [Header("Suavizado")]
    [Tooltip("Velocidad de interpolación hacia la rotación objetivo")]
    public float rotationSmoothSpeed = 8f;

    [Header("Referencia de pantalla")]
    [Tooltip("Si está vacío, se usa toda la pantalla como referencia")]
    public RectTransform screenArea; // opcional: el RawImage donde se muestra el personaje

    private Quaternion baseRotation;
    private Quaternion targetRotation;

    void Start()
    {
        baseRotation = transform.rotation;
        targetRotation = baseRotation;
    }

    void Update()
    {
        Vector2 normalizedMouse = GetNormalizedMousePosition();

        // normalizedMouse va de -1 a 1 en X e Y
        float yaw = normalizedMouse.x * maxYaw;
        float pitch = -normalizedMouse.y * maxPitch; // invertido para que "arriba" incline hacia arriba

        Quaternion offset = Quaternion.Euler(pitch, yaw, 0f);
        targetRotation = baseRotation * offset;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    /// <summary>
    /// Devuelve la posición del mouse normalizada entre -1 y 1,
    /// tomando como centro el centro de la pantalla (o de screenArea si se asignó).
    /// </summary>
    private Vector2 GetNormalizedMousePosition()
    {
        Vector2 mousePos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;

        if (screenArea != null)
        {
            // Convierte la posición del mouse a coordenadas locales del RectTransform
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenArea, mousePos, null, out localPoint);

            Rect rect = screenArea.rect;
            float normX = Mathf.Clamp(localPoint.x / (rect.width * 0.5f), -1f, 1f);
            float normY = Mathf.Clamp(localPoint.y / (rect.height * 0.5f), -1f, 1f);
            return new Vector2(normX, normY);
        }
        else
        {
            float normX = (mousePos.x / Screen.width - 0.5f) * 2f;
            float normY = (mousePos.y / Screen.height - 0.5f) * 2f;
            return new Vector2(
                Mathf.Clamp(normX, -1f, 1f),
                Mathf.Clamp(normY, -1f, 1f)
            );
        }
    }
}

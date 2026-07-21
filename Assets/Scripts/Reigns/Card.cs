using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] Animator animator;
    [SerializeField] string animationParameter;
    [SerializeField] Color positive;
    [SerializeField] Color negative;
    [SerializeField] bool right;

    private void UpdateStatsPreview()
    {
        animator.SetBool(animationParameter, true);
        //SetStatColor(StatManager.Instance.currentStats.a, StatManager.Instance.a);
        //SetStatColor(StatManager.Instance.currentStats.b, StatManager.Instance.b);
        //SetStatColor(StatManager.Instance.currentStats.c, StatManager.Instance.c);
        //SetStatColor(StatManager.Instance.currentStats.d, StatManager.Instance.d);
    }

    private void SetStatColor(int value, TMP_Text text)
    {
        text.color = value > 0
            ? positive
            : value < 0
                ? negative
                : Color.white;
    }

    // Se llama automáticamente cuando el mouse entra al área de este elemento UI
    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateStatsPreview();
    }

    // Se llama automáticamente cuando el mouse sale del área de este elemento UI
    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool(animationParameter, false);
        //StatManager.Instance.a.color = Color.white;
        //StatManager.Instance.b.color = Color.white;
        //StatManager.Instance.c.color = Color.white;
        //StatManager.Instance.d.color = Color.white;
    }

    // Se llama automáticamente al hacer click sobre este elemento UI
    public void OnPointerDown(PointerEventData eventData)
    {
        if (right)
        {
            //StatManager.Instance.AddNewValuesPositive();
        }
        else
        {
            //StatManager.Instance.AddNewValuesNegative();
        }

        UpdateStatsPreview();
    }
}

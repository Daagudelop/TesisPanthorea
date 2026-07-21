using UnityEngine;

public class MousePos : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    }
}

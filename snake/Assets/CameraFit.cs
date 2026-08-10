using UnityEngine;

public class CameraFit : MonoBehaviour
{
    public int gridSize = 11;
    public float padding = 1f; // отступ от краёв поля

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        float targetHalfSize = (gridSize / 2f) + padding;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect >= 1f)
        {
            // альбомная ориентация (шире, чем выше) — высота поля ограничивает
            cam.orthographicSize = targetHalfSize;
        }
        else
        {
            // портретная ориентация (уже, чем выше) — ширина поля ограничивает
            cam.orthographicSize = targetHalfSize / screenAspect;
        }

        transform.position = new Vector3(gridSize / 2f, gridSize / 2f, -10);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class VerticalToggleSlider : MonoBehaviour
{
    public RectTransform handle; // сюда перетащим Checkmark
    public Toggle toggle;        // сюда перетащим сам ControlModeToggle
    public float topPos = 40f;
    public float bottomPos = -40f;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(toggle.isOn);
    }

    void OnToggleChanged(bool isOn)
    {
        Vector2 pos = handle.anchoredPosition;
        pos.y = isOn ? topPos : bottomPos;
        handle.anchoredPosition = pos;
    }
}
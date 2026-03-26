using UnityEngine;

public class CompassMarkerView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public void SetPositionX(float x)
    {
        if (rectTransform == null) return;

        Vector2 pos = rectTransform.anchoredPosition;
        pos.x = x;
        rectTransform.anchoredPosition = pos;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

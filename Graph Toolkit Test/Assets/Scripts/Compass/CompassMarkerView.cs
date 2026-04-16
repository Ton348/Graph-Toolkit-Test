using UnityEngine;

public class CompassMarkerView : MonoBehaviour
{
    [SerializeField] private RectTransform m_rectTransform;

    private void Awake()
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = GetComponent<RectTransform>();
        }
    }

    public void SetPositionX(float x)
    {
        if (m_rectTransform == null) return;

        Vector2 pos = m_rectTransform.anchoredPosition;
        pos.x = x;
        m_rectTransform.anchoredPosition = pos;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

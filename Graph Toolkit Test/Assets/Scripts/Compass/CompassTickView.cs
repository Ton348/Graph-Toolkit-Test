using TMPro;
using UnityEngine;

public class CompassTickView : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private TMP_Text m_label;

	private void Awake()
	{
		if (m_rectTransform == null)
		{
			m_rectTransform = GetComponent<RectTransform>();
		}

		if (m_label == null)
		{
			m_label = GetComponentInChildren<TMP_Text>(true);
		}
	}

	public void SetPositionX(float x)
	{
		if (m_rectTransform == null)
		{
			return;
		}

		Vector2 pos = m_rectTransform.anchoredPosition;
		pos.x = x;
		m_rectTransform.anchoredPosition = pos;
	}

	public void SetVisible(bool visible)
	{
		gameObject.SetActive(visible);
	}

	public void SetLabel(string text)
	{
		if (m_label == null)
		{
			return;
		}

		m_label.text = text;
	}
}
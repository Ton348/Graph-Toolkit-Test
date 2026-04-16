using UnityEngine;
using UnityEngine.UI;

public class BusinessPanelToggle : MonoBehaviour
{
	public GameObject panelRoot;
	public Button openButton;
	public Button closeButton;
	public bool startHidden = true;

	private void Awake()
	{
		if (panelRoot != null && startHidden)
		{
			panelRoot.SetActive(false);
		}

		if (openButton == null)
		{
			openButton = GetComponent<Button>();
		}

		if (openButton != null)
		{
			openButton.onClick.AddListener(Show);
		}

		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Hide);
		}
	}

	public void Show()
	{
		if (panelRoot != null)
		{
			panelRoot.SetActive(true);
		}
	}

	public void Hide()
	{
		if (panelRoot != null)
		{
			panelRoot.SetActive(false);
		}
	}
}
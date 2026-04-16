using UnityEngine;

public class BusinessInteractable : MonoBehaviour
{
	public BusinessWorldRuntime worldRuntime;
	public BusinessPanelController panelController;
	public KeyCode interactKey = KeyCode.E;
	public string playerTag = "Player";

	private void OnTriggerStay(Collider other)
	{
		if (!other.CompareTag(playerTag))
		{
			return;
		}

		if (!Input.GetKeyDown(interactKey))
		{
			return;
		}

		OpenPanel();
	}

	private void OpenPanel()
	{
		if (worldRuntime == null)
		{
			worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
		}

		if (panelController == null)
		{
			panelController = FindObjectOfType<BusinessPanelController>();
		}

		if (panelController == null || worldRuntime == null)
		{
			return;
		}

		panelController.gameObject.SetActive(true);
		panelController.OpenForLot(worldRuntime.lotId);
		BusinessDebugLog.Log($"[BusinessWorld] Open UI lotId='{worldRuntime.lotId}'");
	}
}
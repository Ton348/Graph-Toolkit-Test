using UnityEngine;

public class DeliveryInteractable : MonoBehaviour
{
	public string itemId = "goods";
	public int amount = 1;
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

		var carrier = other.GetComponentInParent<PlayerCarryItem>();
		if (carrier == null)
		{
			carrier = other.GetComponent<PlayerCarryItem>();
		}

		if (carrier == null)
		{
			return;
		}

		carrier.SetItem(itemId, amount);
	}
}
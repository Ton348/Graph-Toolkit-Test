using UnityEngine;

public class Interactable : MonoBehaviour
{
	public float interactionDistance = 2f;

	public virtual void Interact(Transform player)
	{
		Interact();
	}

	public virtual void Interact()
	{
	}

	public bool IsPlayerInRange(Transform player)
	{
		if (player == null)
		{
			return false;
		}

		float distance = Vector3.Distance(player.position, transform.position);
		return distance <= interactionDistance;
	}
}
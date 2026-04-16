using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.E))
		{
			return;
		}

		Interactable[] interactables = FindObjectsOfType<Interactable>();
		Interactable nearest = null;
		var nearestDistance = float.MaxValue;

		foreach (Interactable interactable in interactables)
		{
			if (!interactable.IsPlayerInRange(transform))
			{
				continue;
			}

			float distance = Vector3.Distance(transform.position, interactable.transform.position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = interactable;
			}
		}

		if (nearest != null)
		{
			nearest.Interact(transform);
		}
	}
}
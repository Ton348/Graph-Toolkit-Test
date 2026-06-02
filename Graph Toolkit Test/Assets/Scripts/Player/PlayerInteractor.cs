using UnityEngine;

namespace Sample.Runtime
{
	public class PlayerInteractor : MonoBehaviour
	{
		[SerializeField] private float cacheRefreshSeconds = 1f;

		private Interactable[] m_cachedInteractables = System.Array.Empty<Interactable>();
		private float m_cacheTimer;

		private void UpdateCache()
		{
			m_cachedInteractables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
			m_cacheTimer = 0f;
		}

		private void Update()
		{
			m_cacheTimer += UnityEngine.Time.deltaTime;
			if (m_cachedInteractables.Length == 0 || m_cacheTimer >= Mathf.Max(0.2f, cacheRefreshSeconds))
			{
				UpdateCache();
			}

			if (!Input.GetKeyDown(KeyCode.E))
			{
				return;
			}

			Interactable nearest = null;
			var nearestDistance = float.MaxValue;

			foreach (Interactable interactable in m_cachedInteractables)
			{
				if (interactable == null)
				{
					continue;
				}

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
}

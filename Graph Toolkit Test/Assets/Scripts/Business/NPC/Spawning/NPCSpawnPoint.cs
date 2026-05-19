using UnityEngine;

namespace Prototype.Business.NPC.Spawning
{
	public sealed class NPCSpawnPoint : MonoBehaviour
	{
		[SerializeField] private float minDistanceToPlayer = 20f;
		[SerializeField] private bool avoidMainCamera = true;

		public bool CanSpawn(Transform player, Camera mainCamera)
		{
			if (player != null)
			{
				float sqr = (transform.position - player.position).sqrMagnitude;
				if (sqr < minDistanceToPlayer * minDistanceToPlayer)
				{
					return false;
				}
			}

			if (avoidMainCamera && mainCamera != null)
			{
				Vector3 viewport = mainCamera.WorldToViewportPoint(transform.position);
				if (viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f)
				{
					return false;
				}
			}

			return true;
		}
	}
}

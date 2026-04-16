using UnityEngine;

public class CustomerSpawnPoint : MonoBehaviour
{
	public string lotId;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	public Transform target;
	public Vector3 offset = new(0f, 5f, -7f);
	public float smooth = 5f;
	public float lookAtHeight = 1.6f;

	private void LateUpdate()
	{
		if (target == null)
		{
			return;
		}

		Vector3 desiredPosition = target.position + target.rotation * offset;
		transform.position = Vector3.Lerp(transform.position, desiredPosition, smooth * Time.deltaTime);
		Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
		transform.LookAt(lookAtPoint);
	}
}
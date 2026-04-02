using UnityEngine;

public class QueuePoint : MonoBehaviour
{
    public string lotId;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);
    }
}

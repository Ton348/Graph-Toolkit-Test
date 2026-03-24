using UnityEngine;

public static class StealContextEvaluator
{
    public static bool CanStealFromNpc(Transform player, NPCManager npc)
    {
        if (player == null || npc == null)
        {
            return false;
        }

        if (!npc.allowSteal)
        {
            return false;
        }

        float distance = Vector3.Distance(player.position, npc.transform.position);
        if (distance > npc.stealDistance)
        {
            return false;
        }

        Transform forwardRef = npc.lookForwardReference != null ? npc.lookForwardReference : npc.transform;
        Vector3 toPlayer = (player.position - npc.transform.position);
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        Vector3 directionToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(forwardRef.forward, directionToPlayer);
        float maxAngle = Mathf.Clamp(npc.stealBackAngle, 0f, 180f);
        float minAngle = Mathf.Clamp(npc.stealMinBackAngle, 0f, 180f);
        if (minAngle > maxAngle)
        {
            float tmp = minAngle;
            minAngle = maxAngle;
            maxAngle = tmp;
        }

        bool inside = angle >= minAngle && angle <= maxAngle;
        Debug.Log($"[StealContext] angle={angle:0.0}° distance={distance:0.00} range=[{minAngle:0.0},{maxAngle:0.0}]");
        return inside;
    }
}

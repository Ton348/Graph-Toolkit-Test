using System.Collections.Generic;

public class GraphProgressService
{
    private readonly Dictionary<string, string> m_checkpoints = new Dictionary<string, string>();

    public void SetCheckpoint(string ownerId, string graphId, string checkpointId)
    {
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId) || string.IsNullOrEmpty(checkpointId))
        {
            return;
        }

        m_checkpoints[MakeKey(ownerId, graphId)] = checkpointId;
    }

    public bool TryGetCheckpoint(string ownerId, string graphId, out string checkpointId)
    {
        checkpointId = null;
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            return false;
        }

        return m_checkpoints.TryGetValue(MakeKey(ownerId, graphId), out checkpointId);
    }

    public void ClearCheckpoint(string ownerId, string graphId)
    {
        if (string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(graphId))
        {
            return;
        }

        m_checkpoints.Remove(MakeKey(ownerId, graphId));
    }

    private string MakeKey(string ownerId, string graphId)
    {
        return $"{ownerId}::{graphId}";
    }
}

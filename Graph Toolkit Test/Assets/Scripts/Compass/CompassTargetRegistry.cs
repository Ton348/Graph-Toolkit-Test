using System;
using System.Collections.Generic;
using UnityEngine;

public class CompassTargetRegistry : MonoBehaviour
{
    public static CompassTargetRegistry Instance { get; private set; }

    private readonly Dictionary<string, CompassTarget> m_targets = new Dictionary<string, CompassTarget>();

    public event Action<CompassTarget> targetRegistered;
    public event Action<CompassTarget> targetUnregistered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        m_targets[id] = target;
        targetRegistered?.Invoke(target);
    }

    public void Unregister(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (m_targets.TryGetValue(id, out var existing) && existing == target)
        {
            m_targets.Remove(id);
            targetUnregistered?.Invoke(target);
        }
    }

    public CompassTarget GetTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        m_targets.TryGetValue(id, out var target);
        return target;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class CompassTargetRegistry : MonoBehaviour
{
    public static CompassTargetRegistry Instance { get; private set; }

    private readonly Dictionary<string, CompassTarget> _targets = new Dictionary<string, CompassTarget>();

    public event Action<CompassTarget> TargetRegistered;
    public event Action<CompassTarget> TargetUnregistered;

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

        _targets[id] = target;
        TargetRegistered?.Invoke(target);
    }

    public void Unregister(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (_targets.TryGetValue(id, out var existing) && existing == target)
        {
            _targets.Remove(id);
            TargetUnregistered?.Invoke(target);
        }
    }

    public CompassTarget GetTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        _targets.TryGetValue(id, out var target);
        return target;
    }
}

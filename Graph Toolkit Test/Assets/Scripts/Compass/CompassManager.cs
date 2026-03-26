using System;
using System.Collections.Generic;
using UnityEngine;

public class CompassManager : MonoBehaviour
{
    public static CompassManager Instance { get; private set; }

    [SerializeField] private Transform player;

    private readonly Dictionary<string, CompassTarget> _activeTargets = new Dictionary<string, CompassTarget>();
    private readonly HashSet<string> _pendingShow = new HashSet<string>();

    public event Action ActiveTargetsChanged;

    public Transform Player => player;

    public IReadOnlyCollection<CompassTarget> ActiveTargets => _activeTargets.Values;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        var registry = CompassTargetRegistry.Instance;
        if (registry != null)
        {
            registry.TargetRegistered += OnTargetRegistered;
            registry.TargetUnregistered += OnTargetUnregistered;
        }
    }

    private void OnDisable()
    {
        var registry = CompassTargetRegistry.Instance;
        if (registry != null)
        {
            registry.TargetRegistered -= OnTargetRegistered;
            registry.TargetUnregistered -= OnTargetUnregistered;
        }
    }

    public void ShowTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (_activeTargets.ContainsKey(id)) return;

        var registry = CompassTargetRegistry.Instance;
        var target = registry != null ? registry.GetTarget(id) : null;

        if (target != null)
        {
            _activeTargets[id] = target;
            ActiveTargetsChanged?.Invoke();
        }
        else
        {
            _pendingShow.Add(id);
        }
    }

    public void HideTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        bool removed = _activeTargets.Remove(id);
        _pendingShow.Remove(id);

        if (removed)
        {
            ActiveTargetsChanged?.Invoke();
        }
    }

    private void OnTargetRegistered(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (_pendingShow.Contains(id) || _activeTargets.ContainsKey(id))
        {
            _pendingShow.Remove(id);
            _activeTargets[id] = target;
            ActiveTargetsChanged?.Invoke();
        }
    }

    private void OnTargetUnregistered(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (_activeTargets.TryGetValue(id, out var existing) && existing == target)
        {
            _activeTargets.Remove(id);
            ActiveTargetsChanged?.Invoke();
        }
    }
}

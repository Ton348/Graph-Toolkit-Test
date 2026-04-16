using System;
using System.Collections.Generic;
using UnityEngine;

public class CompassManager : MonoBehaviour
{
    public static CompassManager Instance { get; private set; }

    [SerializeField] private Transform m_player;

    private readonly Dictionary<string, CompassTarget> m_activeTargets = new Dictionary<string, CompassTarget>();
    private readonly HashSet<string> m_pendingShow = new HashSet<string>();

    public event Action activeTargetsChanged;

    public Transform Player => m_player;

    public IReadOnlyCollection<CompassTarget> ActiveTargets => m_activeTargets.Values;

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
            registry.targetRegistered += OnTargetRegistered;
            registry.targetUnregistered += OnTargetUnregistered;
        }
    }

    private void OnDisable()
    {
        var registry = CompassTargetRegistry.Instance;
        if (registry != null)
        {
            registry.targetRegistered -= OnTargetRegistered;
            registry.targetUnregistered -= OnTargetUnregistered;
        }
    }

    public void ShowTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (m_activeTargets.ContainsKey(id)) return;

        var registry = CompassTargetRegistry.Instance;
        var target = registry != null ? registry.GetTarget(id) : null;

        if (target != null)
        {
            m_activeTargets[id] = target;
            activeTargetsChanged?.Invoke();
        }
        else
        {
            m_pendingShow.Add(id);
        }
    }

    public void HideTarget(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        bool removed = m_activeTargets.Remove(id);
        m_pendingShow.Remove(id);

        if (removed)
        {
            activeTargetsChanged?.Invoke();
        }
    }

    private void OnTargetRegistered(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (m_pendingShow.Contains(id) || m_activeTargets.ContainsKey(id))
        {
            m_pendingShow.Remove(id);
            m_activeTargets[id] = target;
            activeTargetsChanged?.Invoke();
        }
    }

    private void OnTargetUnregistered(CompassTarget target)
    {
        if (target == null) return;

        string id = target.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (m_activeTargets.TryGetValue(id, out var existing) && existing == target)
        {
            m_activeTargets.Remove(id);
            activeTargetsChanged?.Invoke();
        }
    }
}

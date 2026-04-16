using System.Collections.Generic;
using UnityEngine;

public class BusinessLiveSimulationService : MonoBehaviour
{
    private GameBootstrap m_bootstrap;
    private BusinessStateSyncService m_stateSync;
    private BusinessSimulationService m_simulation;
    private readonly Dictionary<BusinessWorldRuntime, BusinessWorkerSplineRuntime> m_runtimes = new Dictionary<BusinessWorldRuntime, BusinessWorkerSplineRuntime>();

    public void Initialize(GameBootstrap ownerBootstrap)
    {
        m_bootstrap = ownerBootstrap;
        m_stateSync = m_bootstrap != null ? m_bootstrap.BusinessStateSyncService : null;
        m_simulation = m_bootstrap != null ? m_bootstrap.BusinessSimulationService : null;

        if (m_stateSync != null)
        {
            m_stateSync.stateChanged -= OnStateChanged;
            m_stateSync.stateChanged += OnStateChanged;
        }

        RebuildWorldRuntimes();
        EvaluateAll();
    }

    private void OnDestroy()
    {
        if (m_stateSync != null)
        {
            m_stateSync.stateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged()
    {
        RebuildWorldRuntimes();
        EvaluateAll();
    }

    private void RebuildWorldRuntimes()
    {
        var worlds = FindObjectsByType<BusinessWorldRuntime>(FindObjectsSortMode.None);
        var existing = new HashSet<BusinessWorldRuntime>(worlds);

        foreach (var world in worlds)
        {
            if (world == null || m_runtimes.ContainsKey(world))
            {
                continue;
            }

            var runtime = world.GetComponent<BusinessWorkerSplineRuntime>();
            if (runtime == null)
            {
                runtime = world.gameObject.AddComponent<BusinessWorkerSplineRuntime>();
            }

            runtime.worldRuntime = world;
            runtime.Initialize(m_simulation);
            m_runtimes[world] = runtime;
        }

        var toRemove = new List<BusinessWorldRuntime>();
        foreach (var pair in m_runtimes)
        {
            if (pair.Key == null || !existing.Contains(pair.Key))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            m_runtimes.Remove(key);
        }
    }

    private void EvaluateAll()
    {
        foreach (var runtime in m_runtimes.Values)
        {
            if (runtime != null)
            {
                runtime.EvaluateActivation();
            }
        }
    }
}

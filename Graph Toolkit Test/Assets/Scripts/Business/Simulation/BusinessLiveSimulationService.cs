using System.Collections.Generic;
using UnityEngine;

public class BusinessLiveSimulationService : MonoBehaviour
{
    private GameBootstrap bootstrap;
    private BusinessStateSyncService stateSync;
    private BusinessSimulationService simulation;
    private readonly Dictionary<BusinessWorldRuntime, BusinessWorkerSplineRuntime> runtimes = new Dictionary<BusinessWorldRuntime, BusinessWorkerSplineRuntime>();

    public void Initialize(GameBootstrap ownerBootstrap)
    {
        bootstrap = ownerBootstrap;
        stateSync = bootstrap != null ? bootstrap.BusinessStateSyncService : null;
        simulation = bootstrap != null ? bootstrap.BusinessSimulationService : null;

        if (stateSync != null)
        {
            stateSync.StateChanged -= OnStateChanged;
            stateSync.StateChanged += OnStateChanged;
        }

        RebuildWorldRuntimes();
        EvaluateAll();
    }

    private void OnDestroy()
    {
        if (stateSync != null)
        {
            stateSync.StateChanged -= OnStateChanged;
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
            if (world == null || runtimes.ContainsKey(world))
            {
                continue;
            }

            var runtime = world.GetComponent<BusinessWorkerSplineRuntime>();
            if (runtime == null)
            {
                runtime = world.gameObject.AddComponent<BusinessWorkerSplineRuntime>();
            }

            runtime.worldRuntime = world;
            runtime.Initialize(simulation);
            runtimes[world] = runtime;
        }

        var toRemove = new List<BusinessWorldRuntime>();
        foreach (var pair in runtimes)
        {
            if (pair.Key == null || !existing.Contains(pair.Key))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            runtimes.Remove(key);
        }
    }

    private void EvaluateAll()
    {
        foreach (var runtime in runtimes.Values)
        {
            if (runtime != null)
            {
                runtime.EvaluateActivation();
            }
        }
    }
}

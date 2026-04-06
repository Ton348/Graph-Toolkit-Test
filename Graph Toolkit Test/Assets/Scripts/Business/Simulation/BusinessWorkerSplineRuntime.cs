using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class BusinessWorkerSplineRuntime : MonoBehaviour
{
    public BusinessWorldRuntime worldRuntime;
    public float stopSeconds = 5f;

    private BusinessWorkerSplineAgent activeAgent;
    private BusinessSimulationService simulation;
    private Coroutine activeRoutine;
    private SplineTrigger storageTrigger;
    private SplineTrigger shelvesTrigger;
    private bool triggerListenersBound;

    private void Awake()
    {
        if (worldRuntime == null)
        {
            worldRuntime = GetComponent<BusinessWorldRuntime>();
        }
    }

    public void Initialize(BusinessSimulationService simulationService)
    {
        simulation = simulationService;
        BindSplineTriggers();
    }

    public void EvaluateActivation()
    {
        if (worldRuntime == null || simulation == null)
        {
            DeactivateAgent();
            return;
        }

        var business = worldRuntime.GetBusiness();
        bool hasMerch = business != null && !string.IsNullOrWhiteSpace(business.hiredMerchContactId);
        bool hasStorage = business != null && business.installedModules != null && business.installedModules.Contains("storage");
        bool hasShelves = business != null && business.installedModules != null && business.installedModules.Contains("shelves");
        bool ready = hasMerch && hasStorage && hasShelves && worldRuntime.merchRoute != null && worldRuntime.merchWorkerPrefab != null;

        if (!ready)
        {
            DeactivateAgent();
            return;
        }

        if (activeAgent == null)
        {
            SpawnAgent();
        }
    }

    public void OnTriggerEntered(BusinessSplineTriggerPoint trigger, BusinessWorkerSplineAgent agent)
    {
        if (trigger == null || agent == null || agent != activeAgent || activeRoutine != null)
        {
            return;
        }

        if (trigger.triggerKind == BusinessSplineTriggerPoint.TriggerKind.Storage)
        {
            activeRoutine = StartCoroutine(HandleStorageTrigger());
        }
        else
        {
            activeRoutine = StartCoroutine(HandleShelvesTrigger());
        }
    }

    private void SpawnAgent()
    {
        var spawnTransform = worldRuntime.merchSpawnPoint != null ? worldRuntime.merchSpawnPoint : worldRuntime.transform;
        var spawned = Instantiate(worldRuntime.merchWorkerPrefab, spawnTransform.position, spawnTransform.rotation, worldRuntime.transform);
        activeAgent = spawned.GetComponent<BusinessWorkerSplineAgent>();
        if (activeAgent == null)
        {
            activeAgent = spawned.AddComponent<BusinessWorkerSplineAgent>();
        }

        activeAgent.BindRoute(worldRuntime.merchRoute, spawnTransform, worldRuntime.merchTriggerGroupIndex);
    }

    private IEnumerator HandleStorageTrigger()
    {
        if (activeAgent == null)
        {
            activeRoutine = null;
            yield break;
        }

        activeAgent.StopMovement();
        yield return new WaitForSeconds(stopSeconds);

        while (activeAgent != null)
        {
            var state = simulation.GetStateByLotId(worldRuntime.lotId);
            if (state == null)
            {
                break;
            }

            float shelfSpace = state.shelfCapacity > 0 ? Mathf.Max(0f, state.shelfCapacity - state.shelfStock) : 0f;
            if (state.storageStock <= 0f || shelfSpace <= 0f)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            float request = Mathf.Min(activeAgent.carryBatch, shelfSpace);
            if (simulation.TryTakeFromStorage(worldRuntime.lotId, request, out var taken))
            {
                activeAgent.SetCarry(taken);
            }
            else
            {
                activeAgent.ClearCarry();
            }

            break;
        }

        if (activeAgent != null)
        {
            activeAgent.ResumeMovement();
        }

        activeRoutine = null;
    }

    private IEnumerator HandleShelvesTrigger()
    {
        if (activeAgent == null)
        {
            activeRoutine = null;
            yield break;
        }

        activeAgent.StopMovement();
        yield return new WaitForSeconds(stopSeconds);

        if (activeAgent.CarryingGoods)
        {
            simulation.TryAddToShelves(worldRuntime.lotId, activeAgent.CurrentCarryAmount, out var added);
            if (added > 0f)
            {
                activeAgent.SetCarry(Mathf.Max(0f, activeAgent.CurrentCarryAmount - added));
            }
        }

        activeAgent.ClearCarry();
        activeAgent.ResumeMovement();
        activeRoutine = null;
    }

    private void DeactivateAgent()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (activeAgent != null)
        {
            Destroy(activeAgent.gameObject);
            activeAgent = null;
        }
    }

    private void BindSplineTriggers()
    {
        UnbindSplineTriggers();

        if (worldRuntime == null || worldRuntime.merchRoute == null)
        {
            return;
        }

        var groups = worldRuntime.merchRoute.triggerGroups;
        int groupIndex = worldRuntime.merchTriggerGroupIndex;
        if (groups == null || groupIndex < 0 || groupIndex >= groups.Length || groups[groupIndex] == null || groups[groupIndex].triggers == null)
        {
            return;
        }

        var triggers = groups[groupIndex].triggers;
        for (int i = 0; i < triggers.Length; i++)
        {
            var trigger = triggers[i];
            if (trigger == null)
            {
                continue;
            }

            if (storageTrigger == null && string.Equals(trigger.name, worldRuntime.storageTriggerName, System.StringComparison.Ordinal))
            {
                storageTrigger = trigger;
            }
            else if (shelvesTrigger == null && string.Equals(trigger.name, worldRuntime.shelvesTriggerName, System.StringComparison.Ordinal))
            {
                shelvesTrigger = trigger;
            }
        }

        if (storageTrigger != null)
        {
            storageTrigger.AddListener(OnStorageSplineTrigger);
        }

        if (shelvesTrigger != null)
        {
            shelvesTrigger.AddListener(OnShelvesSplineTrigger);
        }

        triggerListenersBound = storageTrigger != null || shelvesTrigger != null;
    }

    private void UnbindSplineTriggers()
    {
        if (!triggerListenersBound)
        {
            return;
        }

        if (storageTrigger != null)
        {
            storageTrigger.RemoveListener(OnStorageSplineTrigger);
        }

        if (shelvesTrigger != null)
        {
            shelvesTrigger.RemoveListener(OnShelvesSplineTrigger);
        }

        storageTrigger = null;
        shelvesTrigger = null;
        triggerListenersBound = false;
    }

    private void OnDestroy()
    {
        UnbindSplineTriggers();
    }

    private void OnStorageSplineTrigger(SplineUser user)
    {
        if (!IsActiveAgentUser(user) || activeRoutine != null)
        {
            return;
        }

        activeRoutine = StartCoroutine(HandleStorageTrigger());
    }

    private void OnShelvesSplineTrigger(SplineUser user)
    {
        if (!IsActiveAgentUser(user) || activeRoutine != null)
        {
            return;
        }

        activeRoutine = StartCoroutine(HandleShelvesTrigger());
    }

    private bool IsActiveAgentUser(SplineUser user)
    {
        return activeAgent != null && activeAgent.follower != null && user == activeAgent.follower;
    }
}

using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class BusinessWorkerSplineRuntime : MonoBehaviour
{
	public BusinessWorldRuntime worldRuntime;
	public float stopSeconds = 5f;

	private BusinessWorkerSplineAgent m_activeAgent;
	private Coroutine m_activeRoutine;
	private SplineTrigger m_shelvesTrigger;
	private BusinessSimulationService m_simulation;
	private SplineTrigger m_storageTrigger;
	private bool m_triggerListenersBound;

	private void Awake()
	{
		if (worldRuntime == null)
		{
			worldRuntime = GetComponent<BusinessWorldRuntime>();
		}
	}

	private void OnDestroy()
	{
		UnbindSplineTriggers();
	}

	public void Initialize(BusinessSimulationService simulationService)
	{
		m_simulation = simulationService;
		BindSplineTriggers();
	}

	public void EvaluateActivation()
	{
		if (worldRuntime == null || m_simulation == null)
		{
			DeactivateAgent();
			return;
		}

		BusinessInstanceSnapshot business = worldRuntime.GetBusiness();
		bool hasMerch = business != null && !string.IsNullOrWhiteSpace(business.hiredMerchContactId);
		bool hasStorage = business != null && business.installedModules != null &&
		                  business.installedModules.Contains("storage");
		bool hasShelves = business != null && business.installedModules != null &&
		                  business.installedModules.Contains("shelves");
		bool ready = hasMerch && hasStorage && hasShelves && worldRuntime.merchRoute != null &&
		             worldRuntime.merchWorkerPrefab != null;

		if (!ready)
		{
			DeactivateAgent();
			return;
		}

		if (m_activeAgent == null)
		{
			SpawnAgent();
		}
	}

	public void OnTriggerEntered(BusinessSplineTriggerPoint trigger, BusinessWorkerSplineAgent agent)
	{
		if (trigger == null || agent == null || agent != m_activeAgent || m_activeRoutine != null)
		{
			return;
		}

		if (trigger.triggerKind == BusinessSplineTriggerPoint.TriggerKind.Storage)
		{
			m_activeRoutine = StartCoroutine(HandleStorageTrigger());
		}
		else
		{
			m_activeRoutine = StartCoroutine(HandleShelvesTrigger());
		}
	}

	private void SpawnAgent()
	{
		Transform spawnTransform =
			worldRuntime.merchSpawnPoint != null ? worldRuntime.merchSpawnPoint : worldRuntime.transform;
		GameObject spawned = Instantiate(worldRuntime.merchWorkerPrefab, spawnTransform.position,
			spawnTransform.rotation, worldRuntime.transform);
		m_activeAgent = spawned.GetComponent<BusinessWorkerSplineAgent>();
		if (m_activeAgent == null)
		{
			m_activeAgent = spawned.AddComponent<BusinessWorkerSplineAgent>();
		}

		m_activeAgent.BindRoute(worldRuntime.merchRoute, spawnTransform, worldRuntime.merchTriggerGroupIndex);
	}

	private IEnumerator HandleStorageTrigger()
	{
		if (m_activeAgent == null)
		{
			m_activeRoutine = null;
			yield break;
		}

		m_activeAgent.StopMovement();
		yield return new WaitForSeconds(stopSeconds);

		while (m_activeAgent != null)
		{
			BusinessRuntimeSimulationState state = m_simulation.GetStateByLotId(worldRuntime.lotId);
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

			float request = Mathf.Min(m_activeAgent.carryBatch, shelfSpace);
			if (m_simulation.TryTakeFromStorage(worldRuntime.lotId, request, out float taken))
			{
				m_activeAgent.SetCarry(taken);
			}
			else
			{
				m_activeAgent.ClearCarry();
			}

			break;
		}

		if (m_activeAgent != null)
		{
			m_activeAgent.ResumeMovement();
		}

		m_activeRoutine = null;
	}

	private IEnumerator HandleShelvesTrigger()
	{
		if (m_activeAgent == null)
		{
			m_activeRoutine = null;
			yield break;
		}

		m_activeAgent.StopMovement();
		yield return new WaitForSeconds(stopSeconds);

		if (m_activeAgent.CarryingGoods)
		{
			m_simulation.TryAddToShelves(worldRuntime.lotId, m_activeAgent.CurrentCarryAmount, out float added);
			if (added > 0f)
			{
				m_activeAgent.SetCarry(Mathf.Max(0f, m_activeAgent.CurrentCarryAmount - added));
			}
		}

		m_activeAgent.ClearCarry();
		m_activeAgent.ResumeMovement();
		m_activeRoutine = null;
	}

	private void DeactivateAgent()
	{
		if (m_activeRoutine != null)
		{
			StopCoroutine(m_activeRoutine);
			m_activeRoutine = null;
		}

		if (m_activeAgent != null)
		{
			Destroy(m_activeAgent.gameObject);
			m_activeAgent = null;
		}
	}

	private void BindSplineTriggers()
	{
		UnbindSplineTriggers();

		if (worldRuntime == null || worldRuntime.merchRoute == null)
		{
			return;
		}

		TriggerGroup[] groups = worldRuntime.merchRoute.triggerGroups;
		int groupIndex = worldRuntime.merchTriggerGroupIndex;
		if (groups == null || groupIndex < 0 || groupIndex >= groups.Length || groups[groupIndex] == null ||
		    groups[groupIndex].triggers == null)
		{
			return;
		}

		SplineTrigger[] triggers = groups[groupIndex].triggers;
		for (var i = 0; i < triggers.Length; i++)
		{
			SplineTrigger trigger = triggers[i];
			if (trigger == null)
			{
				continue;
			}

			if (m_storageTrigger == null &&
			    string.Equals(trigger.name, worldRuntime.storageTriggerName, StringComparison.Ordinal))
			{
				m_storageTrigger = trigger;
			}
			else if (m_shelvesTrigger == null &&
			         string.Equals(trigger.name, worldRuntime.shelvesTriggerName, StringComparison.Ordinal))
			{
				m_shelvesTrigger = trigger;
			}
		}

		if (m_storageTrigger != null)
		{
			m_storageTrigger.AddListener(OnStorageSplineTrigger);
		}

		if (m_shelvesTrigger != null)
		{
			m_shelvesTrigger.AddListener(OnShelvesSplineTrigger);
		}

		m_triggerListenersBound = m_storageTrigger != null || m_shelvesTrigger != null;
	}

	private void UnbindSplineTriggers()
	{
		if (!m_triggerListenersBound)
		{
			return;
		}

		if (m_storageTrigger != null)
		{
			m_storageTrigger.RemoveListener(OnStorageSplineTrigger);
		}

		if (m_shelvesTrigger != null)
		{
			m_shelvesTrigger.RemoveListener(OnShelvesSplineTrigger);
		}

		m_storageTrigger = null;
		m_shelvesTrigger = null;
		m_triggerListenersBound = false;
	}

	private void OnStorageSplineTrigger(SplineUser user)
	{
		if (!IsActiveAgentUser(user) || m_activeRoutine != null)
		{
			return;
		}

		m_activeRoutine = StartCoroutine(HandleStorageTrigger());
	}

	private void OnShelvesSplineTrigger(SplineUser user)
	{
		if (!IsActiveAgentUser(user) || m_activeRoutine != null)
		{
			return;
		}

		m_activeRoutine = StartCoroutine(HandleShelvesTrigger());
	}

	private bool IsActiveAgentUser(SplineUser user)
	{
		return m_activeAgent != null && m_activeAgent.follower != null && user == m_activeAgent.follower;
	}
}
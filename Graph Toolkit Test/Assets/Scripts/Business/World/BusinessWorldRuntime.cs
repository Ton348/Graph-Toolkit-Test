using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

public class BusinessWorldRuntime : MonoBehaviour
{
	public string siteId;
	public string lotId;
	public Transform storagePoint;
	public Transform shelvesPoint;
	public Transform cashierPoint;
	public Transform deliveryZone;
	public List<BusinessModuleSlot> moduleSlots = new();
	public SplineComputer merchRoute;
	public Transform merchSpawnPoint;
	public GameObject merchWorkerPrefab;
	public int merchTriggerGroupIndex;
	public string storageTriggerName = "Storage";
	public string shelvesTriggerName = "Shelves";

	public GameBootstrap bootstrap;

	private void Awake()
	{
		if (bootstrap == null)
		{
			bootstrap = FindObjectOfType<GameBootstrap>();
		}
	}

	public BusinessInstanceSnapshot GetBusiness()
	{
		return bootstrap != null && bootstrap.BusinessStateSyncService != null
			? bootstrap.BusinessStateSyncService.GetBusinessByLotId(lotId)
			: null;
	}

	public bool IsOwned()
	{
		BusinessInstanceSnapshot business = GetBusiness();
		return business != null && business.isRented;
	}

	public bool IsOpen()
	{
		BusinessInstanceSnapshot business = GetBusiness();
		return business != null && business.isOpen;
	}

	public BusinessActionFacade GetActionFacade()
	{
		return bootstrap != null ? bootstrap.BusinessActionFacade : null;
	}

	public BusinessSimulationService GetSimulationService()
	{
		return bootstrap != null ? bootstrap.BusinessSimulationService : null;
	}

	public BusinessLiveSimulationService GetLiveSimulationService()
	{
		return bootstrap != null ? bootstrap.BusinessLiveSimulationService : null;
	}
}
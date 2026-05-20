using System.Collections.Generic;
using Dreamteck.Splines;
using Prototype.Business.NPC.Registry;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime;
using UnityEngine;

namespace Prototype.Business.World
{
	public class BusinessWorldRuntime : MonoBehaviour
	{
		public string siteId;
		public string lotId;
		[SerializeField] private Transform entrancePoint;
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
		public string BusinessTypeId => GetBusiness()?.businessTypeId;
		public Transform EntrancePoint => entrancePoint;
		public bool IsOpen => GetBusiness() != null && GetBusiness().isOpen;

		private void Awake()
		{
			if (bootstrap == null)
			{
				bootstrap = FindObjectOfType<GameBootstrap>();
			}

			if (entrancePoint == null)
			{
				entrancePoint = transform;
			}
		}

		private void OnEnable()
		{
			BusinessRegistry registry = FindAnyObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
			if (registry != null)
			{
				registry.Register(this);
			}
		}

		private void OnDisable()
		{
			BusinessRegistry registry = FindAnyObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
			if (registry != null)
			{
				registry.Unregister(this);
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
			return business != null;
		}

		public bool IsOpenLegacy() => IsOpen;

		public BusinessActionFacade GetActionFacade()
		{
			return bootstrap != null ? bootstrap.BusinessActionFacade : null;
		}

	}
}

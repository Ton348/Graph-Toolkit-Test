using Prototype.Business.Data;
using Prototype.Business.Bootstrap;
using Prototype.Business.NPC.Needs;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.Runtime;
using Prototype.Business.NPC.Danger;
using Prototype.Business.World;
using System.Threading.Tasks;
using UnityEngine;

namespace Prototype.Business.NPC.Workers
{
	[RequireComponent(typeof(WorkerNPCMovement))]
	public sealed class WorkerNPCBrain : MonoBehaviour
	{
		[SerializeField] private WorkerNPCType workerType;
		[SerializeField] private float waitAtStockSeconds = 2f;
		[SerializeField] private float despawnTravelTimeoutSeconds = 45f;

		private WorkerNPCMovement m_movement;
		private GameBootstrap m_bootstrap;
		private BusinessWorldRuntime m_world;
		private WorkerNPCState m_state;
		private int m_carrying;
		private float m_timer;
		private int m_throughputPerHour;
		private int m_carryCapacity;
		private float m_checkoutSecondsPerItem;
		private float m_lastTransferTime;
		private bool m_registeredCashier;
		private bool m_stockSyncInProgress;
		private bool m_deliveryApplied;
		private Transform m_currentTargetTransform;
		private bool m_isDespawning;
		private float m_despawnStartTime;
		private Vector3 m_despawnTargetPosition;
		private NPCNeedsComponent m_needs;

		public WorkerNPCType WorkerType => workerType;
		public WorkerNPCState CurrentState => m_state;
		public string DebugStateText => m_state switch
		{
			WorkerNPCState.MovingToStorage => "MOVING TO STORAGE",
			WorkerNPCState.CarryingGoods => "CARRYING GOODS",
			WorkerNPCState.RestockingShelves => "RESTOCKING SHELVES",
			WorkerNPCState.WaitingStock => "WAITING STOCK",
			WorkerNPCState.WaitingCustomer => "WAITING CUSTOMER",
			WorkerNPCState.ServingCustomer => "SERVING CUSTOMER",
			WorkerNPCState.Dead => "DEAD",
			_ => m_state.ToString().ToUpperInvariant()
		};

		public void Initialize(BusinessWorldRuntime world, GameBootstrap bootstrap, WorkerNPCType type)
		{
			m_world = world;
			m_bootstrap = bootstrap;
			workerType = type;
			m_movement = GetComponent<WorkerNPCMovement>();
			m_needs = GetComponent<NPCNeedsComponent>();
			if (m_needs != null)
			{
				m_needs.Died -= HandleDeath;
				m_needs.Died += HandleDeath;
			}
			m_state = WorkerNPCState.Spawning;
			ResolveThroughput();
			MoveToEntrance();
		}

		private void OnDestroy()
		{
			if (m_needs != null)
			{
				m_needs.Died -= HandleDeath;
			}
		}

		private void Update()
		{
			if (m_state == WorkerNPCState.Dead)
			{
				return;
			}

			if (m_world == null || m_bootstrap == null || m_movement == null)
			{
				return;
			}

			if (ShouldDespawn())
			{
				BeginDespawn();
				return;
			}

			if (m_isDespawning)
			{
				if (HasReachedTarget())
				{
					Destroy(gameObject);
					return;
				}

				if (!m_movement.HasPath() && m_currentTargetTransform != null)
				{
					m_movement.MoveTo(m_currentTargetTransform.position);
					return;
				}

				if (UnityEngine.Time.time - m_despawnStartTime >= Mathf.Max(5f, despawnTravelTimeoutSeconds))
				{
					transform.position = m_currentTargetTransform != null
						? m_currentTargetTransform.position
						: m_despawnTargetPosition;
					Destroy(gameObject);
					return;
				}

				return;
			}

			switch (workerType)
			{
				case WorkerNPCType.Merchandiser:
					UpdateMerchandiser();
					break;
				case WorkerNPCType.Cashier:
					UpdateCashier();
					break;
				case WorkerNPCType.Logist:
					UpdateLogist();
					break;
			}
		}

		public void HandleDeath()
		{
			m_state = WorkerNPCState.Dead;
			m_movement?.Stop();

			DangerManager manager = DangerManager.Instance;
			if (manager != null)
			{
				manager.RaiseMurderAt(transform.position);
			}
		}

		private void UpdateLogist()
		{
			if (m_state == WorkerNPCState.MovingToEntrance && HasReachedTarget())
			{
				MoveToStorage();
				return;
			}

			if (m_state == WorkerNPCState.MovingToStorage && HasReachedTarget())
			{
				m_state = WorkerNPCState.WaitingStock;
				ApplyDailyDelivery();
				return;
			}

			if (m_state == WorkerNPCState.WaitingStock && m_deliveryApplied)
			{
				BeginDespawn();
				return;
			}
		}

		private void UpdateMerchandiser()
		{
			if (m_state == WorkerNPCState.MovingToEntrance && HasReachedTarget())
			{
				MoveToStorage();
				return;
			}

			if (m_state == WorkerNPCState.MovingToStorage && HasReachedTarget())
			{
				TakeFromStorage();
				if (m_carrying > 0)
				{
					MoveToShelves();
				}
				else
				{
					m_state = WorkerNPCState.WaitingStock;
					m_timer = 0f;
				}
				return;
			}

			if (m_state == WorkerNPCState.WaitingStock)
			{
				m_timer += UnityEngine.Time.deltaTime;
				if (m_timer >= Mathf.Max(0.1f, waitAtStockSeconds))
				{
					m_timer = 0f;
					MoveToStorage();
				}
				return;
			}

			if (m_state == WorkerNPCState.MovingToShelves && HasReachedTarget())
			{
				RestockShelves();
				MoveToStorage();
			}
		}

		private void UpdateCashier()
		{
			if (!m_registeredCashier && !string.IsNullOrWhiteSpace(m_world.lotId))
			{
				WorkerNPCRegistry.SetCashierActive(m_world.lotId, true, this);
				m_registeredCashier = true;
			}

			if (m_state == WorkerNPCState.MovingToEntrance && HasReachedTarget())
			{
				MoveToCashRegister();
				return;
			}

			if (m_state == WorkerNPCState.MovingToCashRegister && HasReachedTarget())
			{
				m_state = WorkerNPCState.WaitingCustomer;
				m_movement.Stop();
			}
		}

		public bool CanServeNow()
		{
			if (workerType != WorkerNPCType.Cashier)
			{
				return false;
			}

			if (m_state != WorkerNPCState.WaitingCustomer && m_state != WorkerNPCState.ServingCustomer)
			{
				return false;
			}

			float minInterval = Mathf.Max(0.1f, m_checkoutSecondsPerItem);
			if (UnityEngine.Time.time - m_lastTransferTime < minInterval)
			{
				return false;
			}

			m_lastTransferTime = UnityEngine.Time.time;
			m_state = WorkerNPCState.ServingCustomer;
			return true;
		}

		private void TakeFromStorage()
		{
			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null)
			{
				m_carrying = 0;
				return;
			}

			int transferCap = Mathf.Max(1, m_carryCapacity);
			if (m_throughputPerHour > 0)
			{
				transferCap = Mathf.Min(transferCap, m_throughputPerHour);
			}

			int available = Mathf.Max(0, business.storageStock);
			m_carrying = Mathf.Min(available, transferCap);
			if (m_carrying > 0)
			{
				m_state = WorkerNPCState.CarryingGoods;
			}
		}

		private void RestockShelves()
		{
			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null || m_bootstrap.BusinessDefinitionsRepository == null || m_carrying <= 0)
			{
				m_carrying = 0;
				return;
			}

			int shelfCap = 0;
			if (!string.IsNullOrWhiteSpace(business.shelfItemId))
			{
				TraderItemDefinitionData item = m_bootstrap.BusinessDefinitionsRepository.GetTraderItem(business.shelfItemId);
				shelfCap = item != null ? Mathf.Max(0, item.shelfCapacity) : 0;
			}

			int shelfStock = Mathf.Max(0, business.shelfStock);
			int free = Mathf.Max(0, shelfCap - shelfStock);
			int add = Mathf.Min(m_carrying, free);
			if (add > 0)
			{
				MoveStockToShelvesAsync(add);
			}

			m_carrying = 0;
			m_state = add > 0 ? WorkerNPCState.RestockingShelves : WorkerNPCState.WaitingStock;
		}

		private async void MoveStockToShelvesAsync(int amount)
		{
			if (amount <= 0 || m_stockSyncInProgress || m_world == null || m_bootstrap == null ||
			    m_bootstrap.BusinessActionFacade == null)
			{
				return;
			}

			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null || string.IsNullOrWhiteSpace(business.lotId))
			{
				return;
			}

			m_stockSyncInProgress = true;
			try
			{
				await m_bootstrap.BusinessActionFacade.MoveBusinessStockToShelf(business.lotId, amount);
			}
			finally
			{
				m_stockSyncInProgress = false;
			}
		}

		private void MoveToEntrance()
		{
			Vector3 point = m_world.EntrancePoint != null ? m_world.EntrancePoint.position : m_world.transform.position;
			m_movement.MoveTo(point);
			m_currentTargetTransform = m_world.EntrancePoint;
			m_state = WorkerNPCState.MovingToEntrance;
		}

		private void MoveToStorage()
		{
			Transform t = m_world.storagePoint != null ? m_world.storagePoint : m_world.EntrancePoint;
			if (t == null)
			{
				return;
			}

			m_movement.MoveTo(t.position);
			m_currentTargetTransform = t;
			m_state = WorkerNPCState.MovingToStorage;
		}

		private void MoveToShelves()
		{
			Transform t = m_world.shelvesPoint != null ? m_world.shelvesPoint : m_world.EntrancePoint;
			if (t == null)
			{
				return;
			}

			m_movement.MoveTo(t.position);
			m_currentTargetTransform = t;
			m_state = WorkerNPCState.MovingToShelves;
		}

		private void MoveToCashRegister()
		{
			Transform t = m_world.cashierPoint != null ? m_world.cashierPoint : m_world.EntrancePoint;
			if (t == null)
			{
				return;
			}

			m_movement.MoveTo(t.position);
			m_currentTargetTransform = t;
			m_state = WorkerNPCState.MovingToCashRegister;
		}

		private bool HasReachedTarget()
		{
			if (m_currentTargetTransform != null)
			{
				Collider c = m_currentTargetTransform.GetComponent<Collider>();
				if (c != null)
				{
					Vector3 closest = c.ClosestPoint(transform.position);
					if ((closest - transform.position).sqrMagnitude <= 0.01f)
					{
						return true;
					}
				}
			}

			return m_movement.HasReached();
		}

		private bool ShouldDespawn()
		{
			if (m_isDespawning)
			{
				return false;
			}

			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null || !business.isOpen)
			{
				return true;
			}

			if (workerType == WorkerNPCType.Merchandiser)
			{
				return string.IsNullOrWhiteSpace(business.hiredMerchContactId) || m_world.storagePoint == null || m_world.shelvesPoint == null;
			}
			if (workerType == WorkerNPCType.Logist)
			{
				if (string.IsNullOrWhiteSpace(business.hiredLogistContactId))
				{
					return true;
				}

				return m_deliveryApplied;
			}
			return string.IsNullOrWhiteSpace(business.hiredCashierContactId) || m_world.cashierPoint == null;
		}

		private void BeginDespawn()
		{
			if (m_isDespawning)
			{
				return;
			}

			if (m_registeredCashier && workerType == WorkerNPCType.Cashier && !string.IsNullOrWhiteSpace(m_world.lotId))
			{
				WorkerNPCRegistry.SetCashierActive(m_world.lotId, false, this);
				m_registeredCashier = false;
			}

			NPCDespawnPoint[] points = FindObjectsByType<NPCDespawnPoint>(FindObjectsSortMode.None);
			Transform best = null;
			float bestDist = float.MaxValue;
			for (int i = 0; i < points.Length; i++)
			{
				NPCDespawnPoint p = points[i];
				if (p == null) continue;
				float d = (p.transform.position - transform.position).sqrMagnitude;
				if (d < bestDist)
				{
					bestDist = d;
					best = p.transform;
				}
			}

			if (best != null)
			{
				m_state = WorkerNPCState.Despawning;
				m_isDespawning = true;
				m_despawnStartTime = UnityEngine.Time.time;
				m_despawnTargetPosition = best.position;
				m_currentTargetTransform = best;
				m_movement.MoveTo(best.position);
			}
			else
			{
				UnityEngine.Debug.LogError($"[WorkerNPCBrain] No NPCDespawnPoint found for {workerType}.");
			}
		}

		private void ResolveThroughput()
		{
			m_throughputPerHour = 0;
			m_carryCapacity = 1;
			m_checkoutSecondsPerItem = 2f;
			if (m_bootstrap == null || m_bootstrap.BusinessDefinitionsRepository == null || m_world == null)
			{
				return;
			}

			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null)
			{
				return;
			}

			string contactId = workerType == WorkerNPCType.Merchandiser ? business.hiredMerchContactId : business.hiredCashierContactId;
			if (string.IsNullOrWhiteSpace(contactId))
			{
				return;
			}

			StaffContactDefinitionData staff = m_bootstrap.BusinessDefinitionsRepository.GetStaffContact(contactId);
			if (staff != null)
			{
				m_throughputPerHour = Mathf.Max(0, staff.throughputPerHour);
				if (staff.carryCapacity > 0)
				{
					m_carryCapacity = staff.carryCapacity;
				}
				if (staff.checkoutSecondsPerItem > 0f)
				{
					m_checkoutSecondsPerItem = staff.checkoutSecondsPerItem;
				}
			}
		}

		private async void ApplyDailyDelivery()
		{
			if (m_deliveryApplied || m_world == null || m_bootstrap == null || m_bootstrap.BusinessActionFacade == null)
			{
				return;
			}

			BusinessInstanceSnapshot business = m_world.GetBusiness();
			if (business == null || string.IsNullOrWhiteSpace(business.lotId))
			{
				return;
			}

			int ordered = Mathf.Max(0, business.autoDeliveryPerDay);
			if (ordered <= 0)
			{
				m_deliveryApplied = true;
				return;
			}

			try
			{
				await m_bootstrap.BusinessActionFacade.ApplyBusinessDelivery(business.lotId, ordered);
				m_deliveryApplied = true;
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogError($"[WorkerNPCBrain] ApplyDailyDelivery failed: {ex}");
				m_deliveryApplied = false;
			}
		}

	}
}

using Prototype.Business.NPC.Movement;
using Prototype.Business.NPC.Needs;
using Prototype.Business.NPC.Registry;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.Runtime;
using Prototype.Business.World;
using System.Threading.Tasks;
using UnityEngine;
using System;

namespace Prototype.Business.NPC.AI
{
	[RequireComponent(typeof(NPCNeedsComponent))]
	[RequireComponent(typeof(NPCMovementController))]
	public sealed class NPCBrain : MonoBehaviour
	{
		public event Action OnTargetReached;
		public event Action<bool> OnServiceFlowCompleted;
		private enum ShoppingState
		{
			None,
			GoingToEntrance,
			GoingToShelves,
			BrowsingShelves,
			GoingToCashier,
			Paying,
			LeavingStore
		}

		[SerializeField] private float decisionTickSeconds = 1f;
		[SerializeField] private float wanderRadius = 25f;
		[SerializeField] private float idleAtTargetSeconds = 2f;
		[SerializeField] private float minimumGoalDurationSeconds = 5f;
		[SerializeField] private float interactionDistance = 1.2f;
		[SerializeField] private float entranceSpreadRadius = 0.8f;
		[SerializeField] private bool graphDrivenOnly = true;

		private NPCNeedsComponent m_needs;
		private NPCMovementController m_movement;
		private NPCGoalResolver m_resolver;
		private BusinessRegistry m_registry;
		private BusinessRuntimeService m_businessRuntime;

		private NPCGoalType m_currentGoal;
		private float m_decisionTimer;
		private float m_goalTimer;
		private float m_idleTimer;
		private bool m_waitingAtTarget;
		private bool m_despawning;
		private bool m_hasTarget;
		private bool m_pausedByDialogue;
		private bool m_resumeMovementAfterDialogue;
		private bool m_isDead;

		private BusinessWorldRuntime m_currentTargetBusiness;
		private string m_currentTargetBusinessName;
		private Vector3 m_currentDestination;
		private Transform m_currentTargetTransform;
		private NPCServiceType m_currentService;
		private ShoppingState m_shoppingState;
		private int m_cartItemCount;
		private bool m_paymentInProgress;

		public NPCGoalType CurrentGoal => m_currentGoal;
		public string CurrentTargetBusinessName => m_currentTargetBusinessName;
		public string CurrentActionLabel => GetActionLabel();
		public NPCNeedsComponent Needs => m_needs;

		private void Awake()
		{
			m_needs = GetComponent<NPCNeedsComponent>();
			m_movement = GetComponent<NPCMovementController>();
			m_resolver = new NPCGoalResolver();
			m_registry = FindFirstObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
			var bootstrap = FindFirstObjectByType<Bootstrap.GameBootstrap>(FindObjectsInactive.Include);
			m_businessRuntime = bootstrap != null ? bootstrap.BusinessRuntimeService : null;
			if (m_needs != null)
			{
				m_needs.Died -= HandleDeath;
				m_needs.Died += HandleDeath;
			}
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
			if (m_isDead)
			{
				return;
			}

			if (m_pausedByDialogue)
			{
				return;
			}

			if (m_despawning)
			{
				return;
			}

			m_decisionTimer += UnityEngine.Time.deltaTime;
			m_goalTimer += UnityEngine.Time.deltaTime;

			if (m_waitingAtTarget)
			{
				m_idleTimer += UnityEngine.Time.deltaTime;
				if (m_idleTimer >= Mathf.Max(0.1f, idleAtTargetSeconds))
				{
					CompleteCurrentInteraction();
				}
				return;
			}

			if (!graphDrivenOnly && m_decisionTimer >= Mathf.Max(0.2f, decisionTickSeconds))
			{
				m_decisionTimer = 0f;
				TickBrain();
			}

			if (m_hasTarget && HasReachedCurrentTarget())
			{
				OnDestinationReached();
			}
		}

		public void BeginDespawn(NPCDespawnPoint[] despawnPoints)
		{
			m_despawning = true;
			if (despawnPoints == null || despawnPoints.Length == 0)
			{
				return;
			}

			float bestDistance = float.MaxValue;
			Transform bestPoint = null;
			for (int i = 0; i < despawnPoints.Length; i++)
			{
				NPCDespawnPoint point = despawnPoints[i];
				if (point == null) continue;
				float sqrDistance = (point.transform.position - transform.position).sqrMagnitude;
				if (sqrDistance < bestDistance)
				{
					bestDistance = sqrDistance;
					bestPoint = point.transform;
				}
			}

			if (bestPoint != null)
			{
				m_hasTarget = true;
				m_currentDestination = bestPoint.position;
				m_movement.MoveTo(bestPoint.position);
			}
		}

		private void TickBrain()
		{
			NPCGoalType resolvedGoal = m_resolver.ResolveGoal(m_needs);
			if (CanSwitchGoal(resolvedGoal))
			{
				SetGoal(resolvedGoal);
			}
			ExecuteCurrentGoal();
		}

		private bool CanSwitchGoal(NPCGoalType newGoal)
		{
			if (newGoal == m_currentGoal) return false;
			if (newGoal == NPCGoalType.Flee) return true;
			return m_goalTimer >= Mathf.Max(1f, minimumGoalDurationSeconds);
		}

		private void SetGoal(NPCGoalType newGoal)
		{
			m_currentGoal = newGoal;
			m_goalTimer = 0f;
			m_currentTargetBusiness = null;
			m_currentTargetBusinessName = null;
			m_currentService = NPCServiceType.Fun;
			m_hasTarget = false;
			m_shoppingState = ShoppingState.None;
			m_cartItemCount = 0;
		}

		private void ExecuteCurrentGoal()
		{
			switch (m_currentGoal)
			{
				case NPCGoalType.GoEat: TryMoveToService(NPCServiceType.Food); break;
				case NPCGoalType.GoDrink: TryMoveToService(NPCServiceType.Drink); break;
				case NPCGoalType.GoToilet: TryMoveToService(NPCServiceType.Toilet); break;
				case NPCGoalType.GoWork: TryMoveToService(NPCServiceType.Work); break;
				case NPCGoalType.GoHome: TryMoveToService(NPCServiceType.Sleep); break;
				default: DoWander(); break;
			}
		}

		private void TryMoveToService(NPCServiceType service)
		{
			if (m_registry == null) m_registry = FindFirstObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
			if (m_businessRuntime == null)
			{
				var bootstrap = FindFirstObjectByType<Bootstrap.GameBootstrap>(FindObjectsInactive.Include);
				m_businessRuntime = bootstrap != null ? bootstrap.BusinessRuntimeService : null;
			}
			if (m_registry == null) return;

			m_currentService = service;
			if (m_currentTargetBusiness != null) return;

			m_currentTargetBusiness = m_registry.FindNearestBusiness(transform.position, service);
			if (m_currentTargetBusiness == null || m_currentTargetBusiness.EntrancePoint == null)
			{
				DoWander();
				return;
			}

				m_currentTargetBusinessName = m_currentTargetBusiness.BusinessTypeId;
				m_currentDestination = GetEntranceDestination(m_currentTargetBusiness.EntrancePoint.position);
				m_currentTargetTransform = m_currentTargetBusiness.EntrancePoint;
				m_hasTarget = true;
			m_shoppingState = ShoppingState.GoingToEntrance;
			m_movement.MoveTo(m_currentDestination);
		}

		private void OnDestinationReached()
		{
			OnTargetReached?.Invoke();
			switch (m_shoppingState)
			{
				case ShoppingState.GoingToEntrance:
					MoveToShelves();
					break;
				case ShoppingState.GoingToShelves:
					BeginShelfBrowse();
					break;
				case ShoppingState.GoingToCashier:
					BeginPayment();
					break;
				case ShoppingState.LeavingStore:
					OnServiceFlowCompleted?.Invoke(true);
					CompleteWithWander();
					break;
				default:
					m_hasTarget = false;
					break;
			}
		}

		public bool MoveToPoint(Vector3 destination)
		{
			m_currentTargetBusiness = null;
			m_currentTargetBusinessName = null;
			m_currentService = NPCServiceType.Fun;
			m_currentTargetTransform = null;
			m_currentDestination = destination;
			m_hasTarget = true;
			m_waitingAtTarget = false;
			m_idleTimer = 0f;
			m_shoppingState = ShoppingState.None;
			m_movement.MoveTo(destination);
			return true;
		}

		public bool StartServiceFlow(NPCServiceType service)
		{
			m_currentGoal = service switch
			{
				NPCServiceType.Food => NPCGoalType.GoEat,
				NPCServiceType.Drink => NPCGoalType.GoDrink,
				NPCServiceType.Toilet => NPCGoalType.GoToilet,
				NPCServiceType.Work => NPCGoalType.GoWork,
				NPCServiceType.Sleep => NPCGoalType.GoHome,
				_ => NPCGoalType.Wander
			};
			m_goalTimer = 0f;
			m_currentTargetBusiness = null;
			m_currentTargetBusinessName = null;
			m_currentTargetTransform = null;
			m_cartItemCount = 0;
			m_shoppingState = ShoppingState.None;
			m_waitingAtTarget = false;
			m_idleTimer = 0f;
			m_hasTarget = false;
			TryMoveToService(service);
			return m_hasTarget || m_currentTargetBusiness != null;
		}

		public void StopMovement()
		{
			m_hasTarget = false;
			m_movement.Stop();
		}

		public void HandleDeath()
		{
			m_isDead = true;
			m_hasTarget = false;
			m_waitingAtTarget = false;
			m_movement.Stop();
		}

		public void PauseForDialogue()
		{
			if (m_pausedByDialogue)
			{
				return;
			}

			m_pausedByDialogue = true;
			m_resumeMovementAfterDialogue = m_hasTarget;
			m_movement.Stop();
		}

		public void ResumeAfterDialogue()
		{
			if (!m_pausedByDialogue)
			{
				return;
			}

			m_pausedByDialogue = false;
			if (m_resumeMovementAfterDialogue && m_hasTarget)
			{
				m_movement.MoveTo(m_currentDestination);
			}

			m_resumeMovementAfterDialogue = false;
		}

		private void MoveToShelves()
		{
			Transform shelves = m_currentTargetBusiness != null ? m_currentTargetBusiness.shelvesPoint : null;
			if (shelves == null)
			{
				LeaveStore();
				return;
			}

			m_currentDestination = shelves.position;
			m_currentTargetTransform = shelves;
			m_hasTarget = true;
			m_shoppingState = ShoppingState.GoingToShelves;
			m_movement.MoveTo(m_currentDestination);
		}

		private void BeginShelfBrowse()
		{
			m_shoppingState = ShoppingState.BrowsingShelves;
			BusinessInstanceSnapshot business = m_currentTargetBusiness != null ? m_currentTargetBusiness.GetBusiness() : null;
			if (business == null || !business.isOpen || business.shelfStock <= 0)
			{
				LeaveStore();
				return;
			}

			int desired = UnityEngine.Random.Range(1, 10);
			m_cartItemCount = desired < business.shelfStock ? desired : business.shelfStock;
			if (m_cartItemCount <= 0)
			{
				LeaveStore();
				return;
			}

			Transform cashier = m_currentTargetBusiness.cashierPoint != null
				? m_currentTargetBusiness.cashierPoint
				: m_currentTargetBusiness.EntrancePoint;
			if (cashier == null)
			{
				LeaveStore();
				return;
			}

			m_currentDestination = cashier.position;
			m_currentTargetTransform = cashier;
			m_hasTarget = true;
			m_shoppingState = ShoppingState.GoingToCashier;
			m_movement.MoveTo(m_currentDestination);
		}

		private void BeginPayment()
		{
			m_waitingAtTarget = true;
			m_idleTimer = 0f;
			m_hasTarget = false;
			m_shoppingState = ShoppingState.Paying;
			m_movement.Stop();
		}

		private void CompleteCurrentInteraction()
		{
			if (m_paymentInProgress)
			{
				return;
			}

			m_waitingAtTarget = false;
			m_idleTimer = 0f;
			m_paymentInProgress = true;
			_ = CompleteCurrentInteractionAsync();
		}

		private async Task CompleteCurrentInteractionAsync()
		{
			bool success = false;
			if (m_currentTargetBusiness != null && m_businessRuntime != null && m_cartItemCount > 0)
			{
				(bool consumeSuccess, int consumed, _) = await m_businessRuntime.TryConsumeServiceAsync(
					m_currentTargetBusiness.lotId,
					m_currentService,
					m_cartItemCount);
				success = consumeSuccess;

				if (success && consumed > 0)
				{
					switch (m_currentGoal)
					{
						case NPCGoalType.GoEat: m_needs.ConsumeNeed(NPCNeedType.Hunger, 100f); break;
						case NPCGoalType.GoDrink: m_needs.ConsumeNeed(NPCNeedType.Thirst, 100f); break;
						case NPCGoalType.GoToilet: m_needs.ConsumeNeed(NPCNeedType.Toilet, 100f); break;
						case NPCGoalType.GoWork: m_needs.ConsumeNeed(NPCNeedType.Work, 100f); break;
						case NPCGoalType.GoHome: m_needs.ConsumeNeed(NPCNeedType.Energy, 100f); break;
					}
				}
			}

			m_cartItemCount = 0;
			if (success)
			{
				LeaveStore();
			}
			else
			{
				ResetCurrentServiceFlow();
				OnServiceFlowCompleted?.Invoke(false);
			}

			m_paymentInProgress = false;
		}

		private void ResetCurrentServiceFlow()
		{
			m_waitingAtTarget = false;
			m_idleTimer = 0f;
			m_hasTarget = false;
			m_cartItemCount = 0;
			m_currentTargetBusiness = null;
			m_currentTargetBusinessName = null;
			m_currentTargetTransform = null;
			m_shoppingState = ShoppingState.None;
		}

		private void LeaveStore()
		{
			if (m_currentTargetBusiness == null || m_currentTargetBusiness.EntrancePoint == null)
			{
				CompleteWithWander();
				OnServiceFlowCompleted?.Invoke(false);
				return;
			}

			m_currentDestination = m_currentTargetBusiness.EntrancePoint.position;
			m_currentTargetTransform = m_currentTargetBusiness.EntrancePoint;
			m_hasTarget = true;
			m_shoppingState = ShoppingState.LeavingStore;
			m_movement.MoveTo(m_currentDestination);
		}

		private void CompleteWithWander()
		{
			ResetCurrentServiceFlow();
		}

		private void DoWander()
		{
			if (m_hasTarget) return;
			if (!m_movement.HasReachedDestination()) return;
			if (m_movement.TryGetRandomNavMeshPoint(transform.position, wanderRadius, out Vector3 point))
			{
				m_hasTarget = true;
				m_currentDestination = point;
				m_currentTargetTransform = null;
				m_currentTargetBusinessName = null;
				m_movement.MoveTo(point);
			}
		}

		private bool HasReachedCurrentTarget()
		{
			if (m_currentTargetTransform != null)
			{
				Prototype.Business.NPC.Locations.NPCLocationPoint locationPoint =
					m_currentTargetTransform.GetComponent<Prototype.Business.NPC.Locations.NPCLocationPoint>();
				if (locationPoint != null)
				{
					float radius = Mathf.Max(interactionDistance, locationPoint.ArrivalRadius);
					float sqrDistance = (transform.position - m_currentDestination).sqrMagnitude;
					if (sqrDistance <= radius * radius)
					{
						return true;
					}
				}

				Collider c = m_currentTargetTransform.GetComponent<Collider>();
				if (c != null)
				{
					Vector3 closest = c.ClosestPoint(transform.position);
					float inSqr = (closest - transform.position).sqrMagnitude;
					if (inSqr <= 0.01f)
					{
						return true;
					}
				}
			}

			float sqr = (transform.position - m_currentDestination).sqrMagnitude;
			if (sqr <= interactionDistance * interactionDistance) return true;
			return m_movement.HasReachedDestination();
		}

		private Vector3 GetEntranceDestination(Vector3 entrancePoint)
		{
			if (entranceSpreadRadius <= 0.01f) return entrancePoint;
			Vector2 offset2d = UnityEngine.Random.insideUnitCircle * entranceSpreadRadius;
			return entrancePoint + new Vector3(offset2d.x, 0f, offset2d.y);
		}

		private string GetActionLabel()
		{
			switch (m_shoppingState)
			{
				case ShoppingState.GoingToEntrance: return "ENTERING STORE";
				case ShoppingState.GoingToShelves:
				case ShoppingState.BrowsingShelves: return "TAKING GOODS";
				case ShoppingState.GoingToCashier: return "GOING TO CASHIER";
				case ShoppingState.Paying: return "PAYING";
				case ShoppingState.LeavingStore: return "LEAVING STORE";
				default: return "WANDERING";
			}
		}
	}
}

using Prototype.Business.NPC.Movement;
using Prototype.Business.NPC.Needs;
using Prototype.Business.NPC.Registry;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.World;
using UnityEngine;

namespace Prototype.Business.NPC.AI
{
	[RequireComponent(typeof(NPCNeedsComponent))]
	[RequireComponent(typeof(NPCMovementController))]
	public sealed class NPCBrain : MonoBehaviour
	{
		[SerializeField] private float decisionTickSeconds = 1f;
		[SerializeField] private float wanderRadius = 25f;
		[SerializeField] private float idleAtTargetSeconds = 2f;

		private NPCNeedsComponent m_needs;
		private NPCMovementController m_movement;
		private NPCGoalResolver m_resolver;
		private BusinessRegistry m_registry;
		private NPCGoalType m_currentGoal;
		private float m_decisionTimer;
		private float m_idleTimer;
		private bool m_waitingAtTarget;
		private bool m_despawning;

		public NPCGoalType CurrentGoal => m_currentGoal;

		private void Awake()
		{
			m_needs = GetComponent<NPCNeedsComponent>();
			m_movement = GetComponent<NPCMovementController>();
			m_resolver = new NPCGoalResolver();
			m_registry = FindAnyObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
		}

		private void Update()
		{
			if (m_despawning)
			{
				return;
			}

			m_decisionTimer += Time.deltaTime;
			if (m_decisionTimer >= Mathf.Max(0.2f, decisionTickSeconds))
			{
				m_decisionTimer = 0f;
				ResolveAndExecuteGoal();
			}

			if (m_waitingAtTarget)
			{
				m_idleTimer += Time.deltaTime;
				if (m_idleTimer >= Mathf.Max(0.1f, idleAtTargetSeconds))
				{
					m_waitingAtTarget = false;
					m_idleTimer = 0f;
				}
			}
		}

		public void BeginDespawn(NPCDespawnPoint[] despawnPoints)
		{
			m_despawning = true;
			if (despawnPoints == null || despawnPoints.Length == 0)
			{
				return;
			}

			float best = float.MaxValue;
			Transform bestPoint = null;
			for (int i = 0; i < despawnPoints.Length; i++)
			{
				NPCDespawnPoint point = despawnPoints[i];
				if (point == null)
				{
					continue;
				}

				float sqr = (point.transform.position - transform.position).sqrMagnitude;
				if (sqr < best)
				{
					best = sqr;
					bestPoint = point.transform;
				}
			}

			if (bestPoint != null)
			{
				m_movement.MoveTo(bestPoint.position);
			}
		}

		private void ResolveAndExecuteGoal()
		{
			m_currentGoal = m_resolver.ResolveGoal(m_needs);
			switch (m_currentGoal)
			{
				case NPCGoalType.GoEat:
					TryMoveToService(NPCServiceType.Food);
					break;
				case NPCGoalType.GoDrink:
					TryMoveToService(NPCServiceType.Drink);
					break;
				case NPCGoalType.GoToilet:
					TryMoveToService(NPCServiceType.Toilet);
					break;
				case NPCGoalType.GoWork:
					TryMoveToService(NPCServiceType.Work);
					break;
				case NPCGoalType.GoHome:
					TryMoveToService(NPCServiceType.Sleep);
					break;
				case NPCGoalType.Flee:
				case NPCGoalType.Wander:
				default:
					DoWander();
					break;
			}
		}

		private void TryMoveToService(NPCServiceType service)
		{
			if (m_registry == null)
			{
				DoWander();
				return;
			}

			BusinessWorldRuntime business = m_registry.FindNearestBusiness(transform.position, service);
			if (business == null || business.EntrancePoint == null)
			{
				DoWander();
				return;
			}

			m_movement.MoveTo(business.EntrancePoint.position);
			if (m_movement.HasReachedDestination())
			{
				m_waitingAtTarget = true;
				m_idleTimer = 0f;
				switch (service)
				{
					case NPCServiceType.Food: m_needs.ConsumeNeed(NPCNeedType.Hunger, 20f); break;
					case NPCServiceType.Drink: m_needs.ConsumeNeed(NPCNeedType.Thirst, 20f); break;
					case NPCServiceType.Toilet: m_needs.ConsumeNeed(NPCNeedType.Toilet, 35f); break;
					case NPCServiceType.Work: m_needs.ConsumeNeed(NPCNeedType.Work, 20f); break;
					case NPCServiceType.Sleep: m_needs.ConsumeNeed(NPCNeedType.Energy, 15f); break;
					case NPCServiceType.Fun: m_needs.ConsumeNeed(NPCNeedType.Fun, 20f); break;
				}
			}
		}

		private void DoWander()
		{
			if (m_waitingAtTarget)
			{
				return;
			}

			if (!m_movement.HasReachedDestination())
			{
				return;
			}

			if (m_movement.TryGetRandomNavMeshPoint(transform.position, wanderRadius, out Vector3 point))
			{
				m_movement.MoveTo(point);
			}
		}
	}
}

	using Prototype.Business.NPC.Movement;
	using Prototype.Business.NPC.Needs;
	using Prototype.Business.NPC.Registry;
	using Prototype.Business.NPC.Spawning;
	using Prototype.Business.Runtime;
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
			[SerializeField] private float minimumGoalDurationSeconds = 5f;
			[SerializeField] private float interactionDistance = 1.2f;
			[SerializeField] private float entranceSpreadRadius = 0.8f;

			private NPCNeedsComponent m_needs;
			private NPCMovementController m_movement;
			private NPCGoalResolver m_resolver;
			private BusinessRegistry m_registry;
			private BusinessRuntimeService m_businessRuntime;

			private NPCGoalType m_currentGoal;
			private NPCGoalType m_previousGoal;

			private float m_decisionTimer;
			private float m_goalTimer;
			private float m_idleTimer;

			private bool m_waitingAtTarget;
			private bool m_despawning;
			private bool m_hasTarget;

			private BusinessWorldRuntime m_currentTargetBusiness;
			private string m_currentTargetBusinessName;

			private Vector3 m_currentDestination;

			public NPCGoalType CurrentGoal => m_currentGoal;
			public string CurrentTargetBusinessName => m_currentTargetBusinessName;

			private void Awake()
			{
				m_needs = GetComponent<NPCNeedsComponent>();
				m_movement = GetComponent<NPCMovementController>();
				m_resolver = new NPCGoalResolver();
				m_registry = FindFirstObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
				var bootstrap = FindFirstObjectByType<Bootstrap.GameBootstrap>(FindObjectsInactive.Include);
				m_businessRuntime = bootstrap != null ? bootstrap.BusinessRuntimeService : null;
			}

			private void Update()
			{
				if (m_despawning)
				{
					return;
				}

				m_decisionTimer += Time.deltaTime;
				m_goalTimer += Time.deltaTime;

				if (m_waitingAtTarget)
				{
					m_idleTimer += Time.deltaTime;
					if (m_idleTimer >= Mathf.Max(0.1f, idleAtTargetSeconds))
					{
						CompleteCurrentInteraction();
					}
					return;
				}

				if (m_decisionTimer >= Mathf.Max(0.2f, decisionTickSeconds))
				{
					m_decisionTimer = 0f;
					TickBrain();
				}

				if (!m_waitingAtTarget &&
				    m_hasTarget &&
				    HasReachedCurrentTarget())
				{
					BeginInteraction();
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
					if (point == null)
					{
						continue;
					}

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
				if (newGoal == m_currentGoal)
				{
					return false;
				}

				if (newGoal == NPCGoalType.Flee)
				{
					return true;
				}

				return m_goalTimer >= Mathf.Max(1f, minimumGoalDurationSeconds);
			}

			private void SetGoal(NPCGoalType newGoal)
			{
				m_previousGoal = m_currentGoal;
				m_currentGoal = newGoal;
				m_goalTimer = 0f;

				m_currentTargetBusiness = null;
				m_currentTargetBusinessName = null;
				m_hasTarget = false;
			}

			private void ExecuteCurrentGoal()
			{
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
					m_registry = FindFirstObjectByType<BusinessRegistry>(FindObjectsInactive.Include);
				}

				if (m_businessRuntime == null)
				{
					var bootstrap = FindFirstObjectByType<Bootstrap.GameBootstrap>(FindObjectsInactive.Include);
					m_businessRuntime = bootstrap != null ? bootstrap.BusinessRuntimeService : null;
				}

				if (m_registry == null)
				{
					return;
				}

				if (m_currentTargetBusiness == null)
				{
					m_currentTargetBusiness = m_registry.FindNearestBusiness(transform.position, service);

					if (m_currentTargetBusiness == null || m_currentTargetBusiness.EntrancePoint == null)
					{
						DoWander();
						return;
					}

					m_currentTargetBusinessName = m_currentTargetBusiness.BusinessTypeId;
					m_currentDestination = GetEntranceDestination(m_currentTargetBusiness.EntrancePoint.position);
					m_hasTarget = true;
					m_movement.MoveTo(m_currentDestination);
				}
			}

			private void BeginInteraction()
			{
				m_hasTarget = false;
				m_currentDestination = Vector3.zero;

				m_waitingAtTarget = true;
				m_idleTimer = 0f;

				m_movement.Stop();
			}

			private void CompleteCurrentInteraction()
			{
				m_waitingAtTarget = false;
				m_idleTimer = 0f;
				m_hasTarget = false;

				NPCServiceType service = MapGoalToService(m_currentGoal);
				bool transactionSuccess = true;
				if (m_currentTargetBusiness != null)
				{
					transactionSuccess = m_businessRuntime != null &&
					                     m_businessRuntime.TryConsumeService(m_currentTargetBusiness.lotId, service, out _);
				}

				if (!transactionSuccess)
				{
					m_currentTargetBusiness = null;
					m_currentTargetBusinessName = null;
					SetGoal(NPCGoalType.Wander);
					return;
				}

				if (transactionSuccess)
				{
					switch (m_currentGoal)
					{
						case NPCGoalType.GoEat:
							m_needs.ConsumeNeed(NPCNeedType.Hunger, 100f);
							break;

						case NPCGoalType.GoDrink:
							m_needs.ConsumeNeed(NPCNeedType.Thirst, 20f);
							break;

						case NPCGoalType.GoToilet:
							m_needs.ConsumeNeed(NPCNeedType.Toilet, 35f);
							break;

						case NPCGoalType.GoWork:
							m_needs.ConsumeNeed(NPCNeedType.Work, 20f);
							break;

						case NPCGoalType.GoHome:
							m_needs.ConsumeNeed(NPCNeedType.Energy, 15f);
							break;
					}
				}

				m_currentTargetBusiness = null;
				m_currentTargetBusinessName = null;

				SetGoal(NPCGoalType.Wander);
			}

			private static NPCServiceType MapGoalToService(NPCGoalType goal)
			{
				switch (goal)
				{
					case NPCGoalType.GoEat: return NPCServiceType.Food;
					case NPCGoalType.GoDrink: return NPCServiceType.Drink;
					case NPCGoalType.GoToilet: return NPCServiceType.Toilet;
					case NPCGoalType.GoWork: return NPCServiceType.Work;
					case NPCGoalType.GoHome: return NPCServiceType.Sleep;
					default: return NPCServiceType.Fun;
				}
			}

			private void DoWander()
			{
				if (m_hasTarget)
				{
					return;
				}

				if (!m_movement.HasReachedDestination())
				{
					return;
				}

				if (m_movement.TryGetRandomNavMeshPoint(transform.position, wanderRadius, out Vector3 point))
				{
					m_hasTarget = true;
					m_currentDestination = point;
					m_currentTargetBusinessName = null;
					m_movement.MoveTo(point);
				}
			}

			private bool HasReachedCurrentTarget()
			{
				float sqr = (transform.position - m_currentDestination).sqrMagnitude;
				if (sqr <= interactionDistance * interactionDistance)
				{
					return true;
				}

				return m_movement.HasReachedDestination();
			}

			private Vector3 GetEntranceDestination(Vector3 entrancePoint)
			{
				if (entranceSpreadRadius <= 0.01f)
				{
					return entrancePoint;
				}

				Vector2 offset2d = Random.insideUnitCircle * entranceSpreadRadius;
				return entrancePoint + new Vector3(offset2d.x, 0f, offset2d.y);
			}
		}
	}

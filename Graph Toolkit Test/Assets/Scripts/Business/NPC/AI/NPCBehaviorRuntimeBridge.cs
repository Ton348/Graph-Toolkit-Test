using Cysharp.Threading.Tasks;
using Prototype.Business.NPC.Movement;
using Prototype.Business.NPC.Locations;
using Prototype.Business.NPC.Needs;
using Prototype.Business.NPC.Registry;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.Time;
using Graph.Core.Runtime.Nodes.Behavior;
using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

namespace Prototype.Business.NPC.AI
{
	[RequireComponent(typeof(NPCBrain))]
	[RequireComponent(typeof(NPCNeedsComponent))]
	public sealed class NPCBehaviorRuntimeBridge : MonoBehaviour
	{
		private NPCBrain m_brain;
		private NPCNeedsComponent m_needs;
		private NPCLocationPoint m_reservedLocation;
		private NPCLocationPoint m_lastVisitedLocation;
		private float m_lastVisitedAt;
		private readonly Dictionary<NPCLocationPoint, float> m_locationCooldownUntil = new Dictionary<NPCLocationPoint, float>();
		private readonly Dictionary<NPCLocationType, int> m_routeIndices = new Dictionary<NPCLocationType, int>();
		private readonly Dictionary<NPCLocationType, bool> m_routeInitialized = new Dictionary<NPCLocationType, bool>();
		private readonly List<NPCLocationPoint> m_routeBuffer = new List<NPCLocationPoint>(16);
		[SerializeField] private float locationMinRepathDistance = 5f;
		[SerializeField] private float locationRepeatCooldownSeconds = 12f;

		private void Awake()
		{
			m_brain = GetComponent<NPCBrain>();
			m_needs = GetComponent<NPCNeedsComponent>();
		}

		public NPCBrain Brain => m_brain;
		public NPCNeedsComponent Needs => m_needs;
		public string DebugActiveBehaviorNode { get; set; }

		public float GetNeedValue(BehaviorNeedType needType)
		{
			if (m_needs == null)
			{
				return 0f;
			}

			switch (needType)
			{
				case BehaviorNeedType.Thirst: return m_needs.Thirst;
				case BehaviorNeedType.Toilet: return m_needs.Toilet;
				case BehaviorNeedType.Fun: return m_needs.Fun;
				case BehaviorNeedType.Energy: return m_needs.Energy;
				case BehaviorNeedType.Work: return m_needs.Work;
				case BehaviorNeedType.Safety: return m_needs.Safety;
				case BehaviorNeedType.City: return m_needs.CityNeed;
				case BehaviorNeedType.Nature: return m_needs.NatureNeed;
				case BehaviorNeedType.Food: return m_needs.FoodNeed;
				default: return m_needs.Hunger;
			}
		}

		public void ModifyNeedValue(BehaviorNeedType needType, float amount, bool increase)
		{
			if (m_needs == null)
			{
				return;
			}

			float delta = Mathf.Abs(amount);
			switch (needType)
			{
				case BehaviorNeedType.Hunger:
					if (increase) { m_needs.AddNeed(NPCNeedType.Hunger, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Hunger, delta); }
					return;
				case BehaviorNeedType.Thirst:
					if (increase) { m_needs.AddNeed(NPCNeedType.Thirst, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Thirst, delta); }
					return;
				case BehaviorNeedType.Toilet:
					if (increase) { m_needs.AddNeed(NPCNeedType.Toilet, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Toilet, delta); }
					return;
				case BehaviorNeedType.Fun:
					if (increase) { m_needs.AddNeed(NPCNeedType.Fun, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Fun, delta); }
					return;
				case BehaviorNeedType.Energy:
					if (increase) { m_needs.AddNeed(NPCNeedType.Energy, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Energy, delta); }
					return;
				case BehaviorNeedType.Work:
					if (increase) { m_needs.AddNeed(NPCNeedType.Work, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Work, delta); }
					return;
				case BehaviorNeedType.Safety:
					float safety = m_needs.Safety + (increase ? delta : -delta);
					m_needs.SetSafety(safety);
					return;
				case BehaviorNeedType.City:
					m_needs.SetCityNeed(m_needs.CityNeed + (increase ? delta : -delta));
					return;
				case BehaviorNeedType.Nature:
					m_needs.SetNatureNeed(m_needs.NatureNeed + (increase ? delta : -delta));
					return;
				case BehaviorNeedType.Food:
					if (increase) { m_needs.AddNeed(NPCNeedType.Hunger, delta); } else { m_needs.ConsumeNeed(NPCNeedType.Hunger, delta); }
					return;
			}
		}

		public async UniTask WaitForHourAsync(int hour, CancellationToken cancellationToken)
		{
			WorldTimeSystem worldTime = WorldTimeSystem.Instance;
			if (worldTime == null)
			{
				return;
			}

			int target = Mathf.Clamp(hour, 0, 23);
			if (Mathf.FloorToInt(worldTime.CurrentHour) == target)
			{
				return;
			}

			var tcs = new UniTaskCompletionSource();
			void Handler(int currentHour)
			{
				if (currentHour == target)
				{
					tcs.TrySetResult();
				}
			}

			worldTime.OnHourChanged += Handler;
			try
			{
				using (cancellationToken.Register(() => tcs.TrySetCanceled()))
				{
					await tcs.Task;
				}
			}
			finally
			{
				worldTime.OnHourChanged -= Handler;
			}
		}

		public async UniTask WaitForEventAsync(string eventName, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(eventName))
			{
				return;
			}

			string normalized = eventName.Trim();
			if (string.Equals(normalized, "NightStarted", StringComparison.OrdinalIgnoreCase))
			{
				WorldTimeSystem worldTime = WorldTimeSystem.Instance;
				if (worldTime == null)
				{
					return;
				}

				if (worldTime.CurrentPhase == DayPhase.Night)
				{
					return;
				}

				var tcs = new UniTaskCompletionSource();
				void Handler(DayPhase phase)
				{
					if (phase == DayPhase.Night)
					{
						tcs.TrySetResult();
					}
				}

				worldTime.OnPhaseChanged += Handler;
				try
				{
					using (cancellationToken.Register(() => tcs.TrySetCanceled()))
					{
						await tcs.Task;
					}
				}
				finally
				{
					worldTime.OnPhaseChanged -= Handler;
				}
			}
		}

		public async UniTask MoveToTargetAndWaitAsync(BehaviorTargetType targetType, CancellationToken cancellationToken)
		{
			if (m_brain == null)
			{
				return;
			}

			if (targetType == BehaviorTargetType.Food)
			{
				bool started = m_brain.StartServiceFlow(NPCServiceType.Food);
				if (!started)
				{
					return;
				}

				var flowTcs = new UniTaskCompletionSource();
				void FlowHandler(bool _)
				{
					flowTcs.TrySetResult();
				}

				m_brain.OnServiceFlowCompleted += FlowHandler;
				try
				{
					using (cancellationToken.Register(() => flowTcs.TrySetCanceled()))
					{
						await flowTcs.Task;
					}
				}
				finally
				{
					m_brain.OnServiceFlowCompleted -= FlowHandler;
				}

				return;
			}

			Vector3 target = transform.position;
			if (targetType == BehaviorTargetType.Despawn)
			{
				NPCDespawnPoint[] points = FindObjectsByType<NPCDespawnPoint>(FindObjectsSortMode.None);
				Transform best = null;
				float bestSqr = float.MaxValue;
				for (int i = 0; i < points.Length; i++)
				{
					NPCDespawnPoint point = points[i];
					if (point == null)
					{
						continue;
					}

					float sqr = (point.transform.position - transform.position).sqrMagnitude;
					if (sqr < bestSqr)
					{
						bestSqr = sqr;
						best = point.transform;
					}
				}

				if (best == null)
				{
					return;
				}

				target = best.position;
			}
			else if (targetType == BehaviorTargetType.Work)
			{
				target = transform.position + transform.forward * 3f;
			}
			else if (targetType == BehaviorTargetType.Home)
			{
				target = transform.position - transform.forward * 3f;
			}
			else if (targetType == BehaviorTargetType.Wander)
			{
				NPCMovementController movement = m_brain != null ? m_brain.GetComponent<NPCMovementController>() : null;
				if (movement == null || !movement.TryGetRandomNavMeshPoint(transform.position, 20f, out target))
				{
					return;
				}
			}
			else if (targetType == BehaviorTargetType.City || targetType == BehaviorTargetType.Park || targetType == BehaviorTargetType.Entertainment)
			{
				NPCLocationRegistry registry = FindFirstObjectByType<NPCLocationRegistry>(FindObjectsInactive.Include);
				if (registry == null)
				{
					return;
				}

				NPCLocationType locationType = targetType switch
				{
					BehaviorTargetType.Park => NPCLocationType.Park,
					BehaviorTargetType.Entertainment => NPCLocationType.Entertainment,
					_ => NPCLocationType.City
				};

				NPCLocationPoint point = ResolveLocationPoint(registry, locationType);
				if (point == null)
				{
					return;
				}

				if (IsPointOnCooldown(point))
				{
					return;
				}

				if (m_reservedLocation != null && m_reservedLocation != point)
				{
					m_reservedLocation.Release();
				}

				m_reservedLocation = point;
				target = point.TargetPoint.position;
			}

			var tcs = new UniTaskCompletionSource();
			void Handler()
			{
				tcs.TrySetResult();
			}

			m_brain.OnTargetReached += Handler;
			m_brain.MoveToPoint(target);
			try
			{
				using (cancellationToken.Register(() => tcs.TrySetCanceled()))
				{
					await tcs.Task;
				}
			}
			finally
			{
				m_brain.OnTargetReached -= Handler;
			}

			if (targetType == BehaviorTargetType.Despawn)
			{
				Destroy(gameObject);
			}
			else if (targetType == BehaviorTargetType.City)
			{
				MarkLocationVisited();
				ReleaseReservedLocation();
			}
			else if (targetType == BehaviorTargetType.Park)
			{
				MarkLocationVisited();
				ReleaseReservedLocation();
			}
			else if (targetType == BehaviorTargetType.Entertainment)
			{
				m_needs?.ConsumeNeed(NPCNeedType.Fun, 20f);
				MarkLocationVisited();
				ReleaseReservedLocation();
			}
			else
			{
				ReleaseReservedLocation();
			}
		}

		private void ReleaseReservedLocation()
		{
			if (m_reservedLocation != null)
			{
				m_reservedLocation.Release();
				m_reservedLocation = null;
			}
		}

		private bool ShouldExcludeLast(NPCLocationType type)
		{
			return m_lastVisitedLocation != null && m_lastVisitedLocation.LocationType == type;
		}

		private NPCLocationPoint ResolveLocationPoint(NPCLocationRegistry registry, NPCLocationType locationType)
		{
			if (registry == null)
			{
				return null;
			}

			if (registry.CollectAvailableByType(locationType, m_routeBuffer) <= 0)
			{
				return null;
			}

			if (!m_routeInitialized.TryGetValue(locationType, out bool initialized) || !initialized)
			{
				int nearestIndex = FindNearestIndex(m_routeBuffer, transform.position);
				if (nearestIndex < 0)
				{
					nearestIndex = 0;
				}

				m_routeIndices[locationType] = nearestIndex;
				m_routeInitialized[locationType] = true;
			}
			else
			{
				int current = m_routeIndices.TryGetValue(locationType, out int idx) ? idx : 0;
				current = (current + 1) % m_routeBuffer.Count;
				m_routeIndices[locationType] = current;
			}

			int targetIndex = m_routeIndices[locationType];
			int count = m_routeBuffer.Count;
			for (int i = 0; i < count; i++)
			{
				NPCLocationPoint candidate = m_routeBuffer[(targetIndex + i) % count];
				if (candidate == null || IsPointOnCooldown(candidate))
				{
					continue;
				}

				if (!candidate.TryReserve())
				{
					continue;
				}

				m_routeIndices[locationType] = (targetIndex + i) % count;
				return candidate;
			}

			return null;
		}

		private static int FindNearestIndex(List<NPCLocationPoint> points, Vector3 from)
		{
			if (points == null || points.Count == 0)
			{
				return -1;
			}

			int nearestIndex = 0;
			float bestSqr = float.MaxValue;
			for (int i = 0; i < points.Count; i++)
			{
				NPCLocationPoint point = points[i];
				if (point == null)
				{
					continue;
				}

				Vector3 target = point.TargetPoint != null ? point.TargetPoint.position : point.transform.position;
				float sqr = (target - from).sqrMagnitude;
				if (sqr < bestSqr)
				{
					bestSqr = sqr;
					nearestIndex = i;
				}
			}

			return nearestIndex;
		}

		private void MarkLocationVisited()
		{
			if (m_reservedLocation == null)
			{
				return;
			}

			m_lastVisitedLocation = m_reservedLocation;
			m_lastVisitedAt = UnityEngine.Time.time;
			m_locationCooldownUntil[m_reservedLocation] = UnityEngine.Time.time + Mathf.Max(0f, locationRepeatCooldownSeconds);
		}

		private bool IsPointOnCooldown(NPCLocationPoint point)
		{
			if (point == null)
			{
				return false;
			}

			if (!m_locationCooldownUntil.TryGetValue(point, out float until))
			{
				return false;
			}

			if (UnityEngine.Time.time >= until)
			{
				m_locationCooldownUntil.Remove(point);
				return false;
			}

			return true;
		}

	}
}

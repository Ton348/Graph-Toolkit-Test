using Cysharp.Threading.Tasks;
using Prototype.Business.NPC.Movement;
using Prototype.Business.NPC.Needs;
using Prototype.Business.NPC.Registry;
using Prototype.Business.NPC.Spawning;
using Prototype.Business.Time;
using Graph.Core.Runtime.Nodes.Behavior;
using System;
using System.Threading;
using UnityEngine;

namespace Prototype.Business.NPC.AI
{
	[RequireComponent(typeof(NPCBrain))]
	[RequireComponent(typeof(NPCNeedsComponent))]
	public sealed class NPCBehaviorRuntimeBridge : MonoBehaviour
	{
		private NPCBrain m_brain;
		private NPCNeedsComponent m_needs;

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
				default: return m_needs.Hunger;
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

		public async UniTask WaitForHungerThresholdAsync(float threshold, CancellationToken cancellationToken)
		{
			if (m_needs == null)
			{
				return;
			}

			float target = Mathf.Clamp(threshold, 0f, 100f);
			if (m_needs.Hunger >= target)
			{
				return;
			}

			var tcs = new UniTaskCompletionSource();
			void Handler(NPCNeedType needType, float value)
			{
				if (needType == NPCNeedType.Hunger && value >= target)
				{
					tcs.TrySetResult();
				}
			}

			m_needs.OnNeedThresholdReached += Handler;
			try
			{
				using (cancellationToken.Register(() => tcs.TrySetCanceled()))
				{
					await tcs.Task;
				}
			}
			finally
			{
				m_needs.OnNeedThresholdReached -= Handler;
			}
		}

		public async UniTask WaitForNeedThresholdAsync(BehaviorNeedType needType, float threshold, CancellationToken cancellationToken)
		{
			if (m_needs == null)
			{
				return;
			}

			float target = Mathf.Clamp(threshold, 0f, 100f);
			if (GetNeedValue(needType) >= target)
			{
				return;
			}

			var tcs = new UniTaskCompletionSource();
			void Handler(NPCNeedType reachedNeedType, float value)
			{
				if (MatchesNeedType(reachedNeedType, needType) && value >= target)
				{
					tcs.TrySetResult();
				}
			}

			m_needs.OnNeedThresholdReached += Handler;
			try
			{
				using (cancellationToken.Register(() => tcs.TrySetCanceled()))
				{
					await tcs.Task;
				}
			}
			finally
			{
				m_needs.OnNeedThresholdReached -= Handler;
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
		}

		private static bool MatchesNeedType(NPCNeedType needType, BehaviorNeedType behaviorNeedType)
		{
			switch (behaviorNeedType)
			{
				case BehaviorNeedType.Thirst: return needType == NPCNeedType.Thirst;
				case BehaviorNeedType.Toilet: return needType == NPCNeedType.Toilet;
				case BehaviorNeedType.Fun: return needType == NPCNeedType.Fun;
				case BehaviorNeedType.Energy: return needType == NPCNeedType.Energy;
				default: return needType == NPCNeedType.Hunger;
			}
		}
	}
}

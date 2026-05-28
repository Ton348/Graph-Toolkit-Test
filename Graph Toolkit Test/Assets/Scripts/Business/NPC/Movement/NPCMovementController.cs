using UnityEngine;
using UnityEngine.AI;
using System;

namespace Prototype.Business.NPC.Movement
{
	[RequireComponent(typeof(NavMeshAgent))]
	public sealed class NPCMovementController : MonoBehaviour
	{
		public enum MovementState
		{
			Idle,
			Moving,
			Stopped
		}

		private NavMeshAgent m_agent;
		private MovementState m_state = MovementState.Idle;
		private System.Random m_random;

		public MovementState CurrentState => m_state;

		private void Awake()
		{
			m_agent = GetComponent<NavMeshAgent>();
			int seed = gameObject.GetInstanceID() ^ Environment.TickCount;
			m_random = new System.Random(seed);
		}

		public void MoveTo(Vector3 point)
		{
			if (m_agent == null || !m_agent.isOnNavMesh)
			{
				return;
			}

			m_agent.isStopped = false;
			m_agent.SetDestination(point);
			m_state = MovementState.Moving;
		}

		public void Stop()
		{
			if (m_agent == null)
			{
				return;
			}

			m_agent.isStopped = true;
			m_state = MovementState.Stopped;
		}

		public bool HasReachedDestination(float tolerance = 0.35f)
		{
			if (m_agent == null || m_agent.pathPending)
			{
				return false;
			}

			if (!m_agent.hasPath)
			{
				if (m_state == MovementState.Moving)
				{
					m_state = MovementState.Idle;
				}
				return true;
			}

			bool reached = m_agent.remainingDistance <= Mathf.Max(0.05f, tolerance);
			if (reached && m_state == MovementState.Moving)
			{
				m_state = MovementState.Idle;
			}

			return reached;
		}

		public bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 point)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = (float)(m_random != null ? m_random.NextDouble() : UnityEngine.Random.value) * Mathf.PI * 2f;
				float distance01 = (float)(m_random != null ? m_random.NextDouble() : UnityEngine.Random.value);
				float distance = Mathf.Sqrt(distance01) * Mathf.Max(1f, radius);
				Vector2 random = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
				Vector3 candidate = origin + new Vector3(random.x, 0f, random.y);
				if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
				{
					point = hit.position;
					return true;
				}
			}

			point = origin;
			return false;
		}
	}
}

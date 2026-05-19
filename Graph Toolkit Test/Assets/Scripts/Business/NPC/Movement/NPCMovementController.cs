using UnityEngine;
using UnityEngine.AI;

namespace Prototype.Business.NPC.Movement
{
	[RequireComponent(typeof(NavMeshAgent))]
	public sealed class NPCMovementController : MonoBehaviour
	{
		private NavMeshAgent m_agent;

		private void Awake()
		{
			m_agent = GetComponent<NavMeshAgent>();
		}

		public void MoveTo(Vector3 point)
		{
			if (m_agent == null || !m_agent.isOnNavMesh)
			{
				return;
			}

			m_agent.isStopped = false;
			m_agent.SetDestination(point);
		}

		public void Stop()
		{
			if (m_agent == null)
			{
				return;
			}

			m_agent.isStopped = true;
		}

		public bool HasReachedDestination(float tolerance = 0.35f)
		{
			if (m_agent == null || m_agent.pathPending)
			{
				return false;
			}

			if (!m_agent.hasPath)
			{
				return true;
			}

			return m_agent.remainingDistance <= Mathf.Max(0.05f, tolerance);
		}

		public bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 point)
		{
			for (int i = 0; i < 8; i++)
			{
				Vector2 random = Random.insideUnitCircle * Mathf.Max(1f, radius);
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

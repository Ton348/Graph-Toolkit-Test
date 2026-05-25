using UnityEngine;
using UnityEngine.AI;

namespace Prototype.Business.NPC.Workers
{
	[RequireComponent(typeof(NavMeshAgent))]
	public sealed class WorkerNPCMovement : MonoBehaviour
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

		public bool WarpToNavMesh(Vector3 preferredPoint, float maxDistance = 5f)
		{
			if (m_agent == null)
			{
				return false;
			}

			if (NavMesh.SamplePosition(preferredPoint, out NavMeshHit hit, Mathf.Max(0.5f, maxDistance), NavMesh.AllAreas))
			{
				return m_agent.Warp(hit.position);
			}

			return false;
		}

		public void Stop()
		{
			if (m_agent == null)
			{
				return;
			}

			m_agent.isStopped = true;
		}

		public bool HasReached(float tolerance = 0.4f)
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

		public bool HasPath()
		{
			if (m_agent == null || !m_agent.isOnNavMesh)
			{
				return false;
			}

			return m_agent.pathPending || m_agent.hasPath;
		}
	}
}

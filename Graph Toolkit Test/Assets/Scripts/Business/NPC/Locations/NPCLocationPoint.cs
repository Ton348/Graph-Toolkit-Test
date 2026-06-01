using Prototype.Business.Time;
using UnityEngine;
using UnityEngine.AI;

namespace Prototype.Business.NPC.Locations
{
	public sealed class NPCLocationPoint : MonoBehaviour
	{
		[SerializeField] private NPCLocationType locationType = NPCLocationType.City;
		[SerializeField] private int capacity = 6;
		[SerializeField] private float openHour;
		[SerializeField] private float closeHour = 24f;
		[SerializeField] private Transform targetPoint;
		[SerializeField] private float arrivalRadius = 0f;

		private int m_occupancy;

		public NPCLocationType LocationType => locationType;
		public int Capacity => Mathf.Max(1, capacity);
		public Transform TargetPoint => targetPoint != null ? targetPoint : transform;
		public float ArrivalRadius => Mathf.Max(0f, arrivalRadius);
		public int Occupancy => m_occupancy;

		public bool IsOpenNow()
		{
			WorldTimeSystem time = WorldTimeSystem.Instance;
			if (time == null)
			{
				return true;
			}

			return time.IsBusinessHours(openHour, closeHour);
		}

		public bool TryReserve()
		{
			if (!IsOpenNow() || m_occupancy >= Capacity)
			{
				return false;
			}

			m_occupancy++;
			return true;
		}

		public void Release()
		{
			if (m_occupancy > 0)
			{
				m_occupancy--;
			}
		}

		public bool TryGetArrivalPoint(out Vector3 point)
		{
			Vector3 center = TargetPoint.position;
			float radius = ArrivalRadius;
			if (radius <= 0.01f)
			{
				if (NavMesh.SamplePosition(center, out NavMeshHit hitCenter, 2f, NavMesh.AllAreas))
				{
					point = hitCenter.position;
					return true;
				}

				point = center;
				return false;
			}

			for (int i = 0; i < 8; i++)
			{
				Vector2 random = Random.insideUnitCircle * radius;
				Vector3 candidate = center + new Vector3(random.x, 0f, random.y);
				if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
				{
					point = hit.position;
					return true;
				}
			}

			if (NavMesh.SamplePosition(center, out NavMeshHit fallbackHit, 2f, NavMesh.AllAreas))
			{
				point = fallbackHit.position;
				return true;
			}

			point = center;
			return false;
		}
	}
}

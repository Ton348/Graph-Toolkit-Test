using Prototype.Business.Time;
using UnityEngine;

namespace Prototype.Business.NPC.Locations
{
	public sealed class NPCLocationPoint : MonoBehaviour
	{
		[SerializeField] private NPCLocationType locationType = NPCLocationType.City;
		[SerializeField] private int capacity = 6;
		[SerializeField] private float openHour;
		[SerializeField] private float closeHour = 24f;
		[SerializeField] private Transform targetPoint;

		private int m_occupancy;

		public NPCLocationType LocationType => locationType;
		public int Capacity => Mathf.Max(1, capacity);
		public Transform TargetPoint => targetPoint != null ? targetPoint : transform;
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
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Business.NPC.Locations
{
	public sealed class NPCLocationRegistry : MonoBehaviour
	{
		private readonly List<NPCLocationPoint> m_points = new List<NPCLocationPoint>();

		private void Awake()
		{
			Rebuild();
		}

		private void OnEnable()
		{
			Rebuild();
		}

		public void Rebuild()
		{
			m_points.Clear();
			NPCLocationPoint[] all = FindObjectsByType<NPCLocationPoint>(FindObjectsSortMode.None);
			for (int i = 0; i < all.Length; i++)
			{
				NPCLocationPoint p = all[i];
				if (p != null)
				{
					m_points.Add(p);
				}
			}
		}

		public NPCLocationPoint FindNearestAvailable(Vector3 from, NPCLocationType type)
		{
			NPCLocationPoint best = null;
			float bestSqr = float.MaxValue;
			for (int i = 0; i < m_points.Count; i++)
			{
				NPCLocationPoint p = m_points[i];
				if (p == null || p.LocationType != type || !p.IsOpenNow())
				{
					continue;
				}

				if (p.Occupancy >= p.Capacity)
				{
					continue;
				}

				float sqr = (p.TargetPoint.position - from).sqrMagnitude;
				if (sqr < bestSqr)
				{
					bestSqr = sqr;
					best = p;
				}
			}

			return best;
		}

		public NPCLocationPoint FindNearestAvailable(
			Vector3 from,
			NPCLocationType type,
			NPCLocationPoint exclude,
			float minDistance)
		{
			NPCLocationPoint best = null;
			float bestSqr = float.MaxValue;
			float minDistanceSqr = Mathf.Max(0f, minDistance) * Mathf.Max(0f, minDistance);
			for (int i = 0; i < m_points.Count; i++)
			{
				NPCLocationPoint p = m_points[i];
				if (p == null || p == exclude || p.LocationType != type || !p.IsOpenNow())
				{
					continue;
				}

				if (p.Occupancy >= p.Capacity)
				{
					continue;
				}

				float sqr = (p.TargetPoint.position - from).sqrMagnitude;
				if (sqr < minDistanceSqr)
				{
					continue;
				}

				if (sqr < bestSqr)
				{
					bestSqr = sqr;
					best = p;
				}
			}

			return best;
		}

		public int CollectAvailableByType(NPCLocationType type, List<NPCLocationPoint> resultBuffer)
		{
			if (resultBuffer == null)
			{
				return 0;
			}

			resultBuffer.Clear();
			for (int i = 0; i < m_points.Count; i++)
			{
				NPCLocationPoint p = m_points[i];
				if (p == null || p.LocationType != type || !p.IsOpenNow())
				{
					continue;
				}

				if (p.Occupancy >= p.Capacity)
				{
					continue;
				}

				resultBuffer.Add(p);
			}

			resultBuffer.Sort(CompareByName);
			return resultBuffer.Count;
		}

		private static int CompareByName(NPCLocationPoint a, NPCLocationPoint b)
		{
			if (a == b)
			{
				return 0;
			}

			if (a == null)
			{
				return 1;
			}

			if (b == null)
			{
				return -1;
			}

			return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
		}
	}
}

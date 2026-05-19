using System.Collections.Generic;
using Prototype.Business.World;
using UnityEngine;

namespace Prototype.Business.NPC.Registry
{
	public sealed class BusinessRegistry : MonoBehaviour
	{
		private readonly List<BusinessWorldRuntime> m_runtimes = new();
		private readonly List<BusinessWorldRuntime> m_resultBuffer = new();

		public void Register(BusinessWorldRuntime runtime)
		{
			if (runtime == null || m_runtimes.Contains(runtime))
			{
				return;
			}

			m_runtimes.Add(runtime);
		}

		public void Unregister(BusinessWorldRuntime runtime)
		{
			if (runtime == null)
			{
				return;
			}

			m_runtimes.Remove(runtime);
		}

		public IReadOnlyList<BusinessWorldRuntime> GetBusinessesByService(NPCServiceType service)
		{
			m_resultBuffer.Clear();
			for (int i = 0; i < m_runtimes.Count; i++)
			{
				BusinessWorldRuntime runtime = m_runtimes[i];
				if (runtime == null)
				{
					continue;
				}

				IReadOnlyList<NPCServiceType> services = ResolveServices(runtime);
				for (int s = 0; s < services.Count; s++)
				{
					if (services[s] == service)
					{
						m_resultBuffer.Add(runtime);
						break;
					}
				}
			}

			return m_resultBuffer;
		}

		public BusinessWorldRuntime FindNearestBusiness(Vector3 position, NPCServiceType service)
		{
			BusinessWorldRuntime nearest = null;
			float bestDistanceSqr = float.MaxValue;
			for (int i = 0; i < m_runtimes.Count; i++)
			{
				BusinessWorldRuntime runtime = m_runtimes[i];
				if (runtime == null || !runtime.IsOpen || runtime.EntrancePoint == null)
				{
					continue;
				}

				IReadOnlyList<NPCServiceType> services = ResolveServices(runtime);
				bool hasService = false;
				for (int s = 0; s < services.Count; s++)
				{
					if (services[s] == service)
					{
						hasService = true;
						break;
					}
				}

				if (!hasService)
				{
					continue;
				}

				float dist = (runtime.EntrancePoint.position - position).sqrMagnitude;
				if (dist < bestDistanceSqr)
				{
					bestDistanceSqr = dist;
					nearest = runtime;
				}
			}

			return nearest;
		}

		private static IReadOnlyList<NPCServiceType> ResolveServices(BusinessWorldRuntime runtime)
		{
			if (runtime == null)
			{
				return System.Array.Empty<NPCServiceType>();
			}

			string businessTypeId = runtime.BusinessTypeId;
			if (string.IsNullOrWhiteSpace(businessTypeId))
			{
				return System.Array.Empty<NPCServiceType>();
			}

			var definitions = runtime.bootstrap != null ? runtime.bootstrap.BusinessDefinitionsRepository : null;
			if (definitions == null)
			{
				return System.Array.Empty<NPCServiceType>();
			}

			return definitions.GetServicesForBusinessType(businessTypeId);
		}
	}
}

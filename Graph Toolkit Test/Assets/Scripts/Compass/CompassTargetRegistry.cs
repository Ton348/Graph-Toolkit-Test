using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sample.Runtime.Compass
{
	public class CompassTargetRegistry : MonoBehaviour
	{
		private readonly Dictionary<string, CompassTarget> m_targets = new();
		public static CompassTargetRegistry Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		public event Action<CompassTarget> targetRegistered;
		public event Action<CompassTarget> targetUnregistered;

		public void Register(CompassTarget target)
		{
			if (target == null)
			{
				return;
			}

			string id = target.TargetId;
			if (string.IsNullOrWhiteSpace(id))
			{
				return;
			}

			m_targets[id] = target;
			targetRegistered?.Invoke(target);
		}

		public void Unregister(CompassTarget target)
		{
			if (target == null)
			{
				return;
			}

			string id = target.TargetId;
			if (string.IsNullOrWhiteSpace(id))
			{
				return;
			}

			if (m_targets.TryGetValue(id, out CompassTarget existing) && existing == target)
			{
				m_targets.Remove(id);
				targetUnregistered?.Invoke(target);
			}
		}

		public CompassTarget GetTarget(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			m_targets.TryGetValue(id, out CompassTarget target);
			return target;
		}
	}
}
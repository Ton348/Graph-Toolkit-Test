using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Business.World
{
	public class BusinessVisualRegistry : MonoBehaviour
	{
		[SerializeField]
		private List<Entry> m_entries = new();

		private readonly Dictionary<string, GameObject> m_prefabByVisualId = new();
		private bool m_cacheDirty = true;

		public int EntryCount => m_entries != null ? m_entries.Count : 0;

		private void OnValidate()
		{
			m_cacheDirty = true;
		}

		public GameObject GetPrefab(string visualId)
		{
			if (string.IsNullOrWhiteSpace(visualId))
			{
				return null;
			}

			RebuildCacheIfNeeded();
			m_prefabByVisualId.TryGetValue(visualId.Trim(), out GameObject prefab);
			return prefab;
		}

		public bool HasEntries()
		{
			RebuildCacheIfNeeded();
			return m_prefabByVisualId.Count > 0;
		}

		private void RebuildCacheIfNeeded()
		{
			if (!m_cacheDirty)
			{
				return;
			}

			m_prefabByVisualId.Clear();
			foreach (Entry entry in m_entries)
			{
				if (entry == null || string.IsNullOrWhiteSpace(entry.visualId) || entry.prefab == null)
				{
					continue;
				}

				string visualId = entry.visualId.Trim();
				if (!m_prefabByVisualId.ContainsKey(visualId))
				{
					m_prefabByVisualId.Add(visualId, entry.prefab);
				}
			}

			m_cacheDirty = false;
		}

		[Serializable]
		public class Entry
		{
			public string visualId;
			public GameObject prefab;
		}
	}
}
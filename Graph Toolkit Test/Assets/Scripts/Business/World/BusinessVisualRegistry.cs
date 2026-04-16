using System;
using System.Collections.Generic;
using UnityEngine;

public class BusinessVisualRegistry : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string visualId;
        public GameObject prefab;
    }

    [SerializeField] private List<Entry> m_entries = new List<Entry>();

    private readonly Dictionary<string, GameObject> m_prefabByVisualId = new Dictionary<string, GameObject>();
    private bool m_cacheDirty = true;

    public int EntryCount => m_entries != null ? m_entries.Count : 0;

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

    private void OnValidate()
    {
        m_cacheDirty = true;
    }

    private void RebuildCacheIfNeeded()
    {
        if (!m_cacheDirty)
        {
            return;
        }

        m_prefabByVisualId.Clear();
        foreach (var entry in m_entries)
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
}

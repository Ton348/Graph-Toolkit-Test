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

    [SerializeField] private List<Entry> entries = new List<Entry>();

    private readonly Dictionary<string, GameObject> prefabByVisualId = new Dictionary<string, GameObject>();
    private bool cacheDirty = true;

    public int EntryCount => entries != null ? entries.Count : 0;

    public GameObject GetPrefab(string visualId)
    {
        if (string.IsNullOrWhiteSpace(visualId))
        {
            return null;
        }

        RebuildCacheIfNeeded();
        prefabByVisualId.TryGetValue(visualId.Trim(), out GameObject prefab);
        return prefab;
    }

    public bool HasEntries()
    {
        RebuildCacheIfNeeded();
        return prefabByVisualId.Count > 0;
    }

    private void OnValidate()
    {
        cacheDirty = true;
    }

    private void RebuildCacheIfNeeded()
    {
        if (!cacheDirty)
        {
            return;
        }

        prefabByVisualId.Clear();
        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.visualId) || entry.prefab == null)
            {
                continue;
            }

            string visualId = entry.visualId.Trim();
            if (!prefabByVisualId.ContainsKey(visualId))
            {
                prefabByVisualId.Add(visualId, entry.prefab);
            }
        }

        cacheDirty = false;
    }
}

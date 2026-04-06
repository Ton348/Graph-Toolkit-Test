using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BusinessListView : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    private readonly List<BusinessInstanceSnapshot> entries = new List<BusinessInstanceSnapshot>();
    public event Action<BusinessInstanceSnapshot> SelectionChanged;

    private void Awake()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    public void SetBusinesses(IEnumerable<BusinessInstanceSnapshot> businesses, Func<BusinessInstanceSnapshot, string> labelProvider = null)
    {
        entries.Clear();
        if (dropdown == null)
        {
            return;
        }

        dropdown.ClearOptions();
        var labels = new List<string>();

        if (businesses != null)
        {
            foreach (var business in businesses)
            {
                if (business == null) continue;
                entries.Add(business);
                string label = labelProvider != null ? labelProvider(business) : null;
                if (string.IsNullOrWhiteSpace(label))
                {
                    label = !string.IsNullOrWhiteSpace(business.lotId)
                        ? business.lotId
                        : business.instanceId;
                }
                labels.Add(label);
            }
        }

        if (labels.Count == 0)
        {
            labels.Add("No businesses");
        }

        dropdown.AddOptions(labels);
        dropdown.value = 0;
        OnDropdownChanged(0);
    }

    private void OnDropdownChanged(int index)
    {
        if (entries.Count == 0)
        {
            SelectionChanged?.Invoke(null);
            return;
        }

        if (index < 0 || index >= entries.Count)
        {
            SelectionChanged?.Invoke(null);
            return;
        }

        SelectionChanged?.Invoke(entries[index]);
    }
}

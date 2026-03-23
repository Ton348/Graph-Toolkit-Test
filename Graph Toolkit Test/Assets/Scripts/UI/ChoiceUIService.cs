using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUIService : MonoBehaviour
{
    public GameObject panel;
    public Transform optionsRoot;
    public Button optionButtonPrefab;

    private Action<string> onSelect;
    private readonly List<Button> spawnedButtons = new List<Button>();

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(List<ChoiceOption> options, Action<string> onSelectCallback)
    {
        if (optionsRoot == null || optionButtonPrefab == null)
        {
            return;
        }

        ClearButtons();
        onSelect = onSelectCallback;

        foreach (ChoiceOption option in options)
        {
            if (option == null)
            {
                continue;
            }

            Button btn = Instantiate(optionButtonPrefab, optionsRoot);
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = option.label;
            }

            string optionId = option.optionId;
            btn.onClick.AddListener(() => HandleSelect(optionId));
            spawnedButtons.Add(btn);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void HandleSelect(string optionId)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        var callback = onSelect;
        onSelect = null;
        ClearButtons();
        callback?.Invoke(optionId);
    }

    private void ClearButtons()
    {
        foreach (Button btn in spawnedButtons)
        {
            if (btn != null)
            {
                Destroy(btn.gameObject);
            }
        }

        spawnedButtons.Clear();
    }
}

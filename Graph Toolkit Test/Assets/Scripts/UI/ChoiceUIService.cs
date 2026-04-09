using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUIService : MonoBehaviour, IGraphChoiceService
{
    public Task<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options)
    {
        var tcs = new TaskCompletionSource<int>();
        var legacyOptions = new List<ChoiceOption>();
        if (options != null)
        {
            for (int i = 0; i < options.Count; i++)
            {
                legacyOptions.Add(new ChoiceOption
                {
                    label = options[i].Label
                });
            }
        }

        ShowChoices(legacyOptions, index => tcs.TrySetResult(index));
        return tcs.Task;
    }

    public GameObject panel;
    public Button[] optionButtons = new Button[4];
    public TMP_Text[] optionTexts = new TMP_Text[4];

    private Action<int> onSelected;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowChoices(List<ChoiceOption> options, Action<int> onSelectedCallback)
    {
        onSelected = onSelectedCallback;
        ApplyOptions(options);
    }

    private void ApplyOptions(List<ChoiceOption> options)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        for (int i = 0; i < 4; i++)
        {
            Button button = (optionButtons != null && i < optionButtons.Length) ? optionButtons[i] : null;
            TMP_Text label = (optionTexts != null && i < optionTexts.Length) ? optionTexts[i] : null;

            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();

            ChoiceOption option = (options != null && i < options.Count) ? options[i] : null;
            if (option == null || string.IsNullOrWhiteSpace(option.label))
            {
                button.gameObject.SetActive(false);
                continue;
            }

            button.gameObject.SetActive(true);

            if (label != null)
            {
                label.text = option.label ?? string.Empty;
            }

            int index = i;
            button.onClick.AddListener(() => HandleSelect(index));
        }
    }

    private void HandleSelect(int index)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        var callback = onSelected;
        onSelected = null;
        callback?.Invoke(index);
    }

    public void HideChoices()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        onSelected = null;

        if (optionButtons == null)
        {
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            Button button = optionButtons[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}

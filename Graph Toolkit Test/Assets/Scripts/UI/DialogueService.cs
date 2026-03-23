using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueService : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button continueButton;

    private Action onContinue;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinue);
        }
    }

    public void Show(string title, string body, Action continueCallback)
    {
        onContinue = continueCallback;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void HandleContinue()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        var callback = onContinue;
        onContinue = null;
        callback?.Invoke();
    }
}

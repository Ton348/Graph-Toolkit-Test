using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIService : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Image screenshotImage;
    public Button continueButton;
    public bool hidePanelOnContinue = false;

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

    private void OnDisable()
    {
        HideDialogue();
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf || onContinue == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleContinue();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleContinue();
        }
    }

    public void ShowDialogue(string title, string body, Action continueCallback, Sprite screenshot = null)
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

        if (screenshotImage != null)
        {
            screenshotImage.sprite = screenshot;
            screenshotImage.gameObject.SetActive(screenshot != null);
            if (screenshot != null)
            {
                screenshotImage.SetNativeSize();
            }
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void HideDialogue()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        onContinue = null;
    }

    private void HandleContinue()
    {
        if (hidePanelOnContinue && panel != null)
        {
            panel.SetActive(false);
        }

        var callback = onContinue;
        onContinue = null;
        callback?.Invoke();
    }
}

public class DialogueService : DialogueUIService, IGraphDialogueService
{
    public UniTask ShowAsync(string title, string body, CancellationToken cancellationToken)
    {
        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
        CancellationTokenRegistration registration = cancellationToken.Register(() => completionSource.TrySetCanceled());
        ShowDialogue(title, body, () => completionSource.TrySetResult(), null);
        return AwaitWithCleanupAsync(this, completionSource, registration);
    }

    public void EndConversation()
    {
        HideDialogue();
    }

    private static async UniTask AwaitWithCleanupAsync(DialogueUIService dialogueUIService, UniTaskCompletionSource completionSource, CancellationTokenRegistration registration)
    {
        try
        {
            await completionSource.Task;
        }
        finally
        {
            registration.Dispose();
        }
    }
}

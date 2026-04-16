using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUiservice : MonoBehaviour
{
	public GameObject panel;
	public TMP_Text titleText;
	public TMP_Text bodyText;
	public Image screenshotImage;
	public Button continueButton;
	public bool hidePanelOnContinue;

	private Action m_onContinue;

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

	private void Update()
	{
		if (panel == null || !panel.activeSelf || m_onContinue == null)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
		    Input.GetKeyDown(KeyCode.Space))
		{
			HandleContinue();
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			HandleContinue();
		}
	}

	private void OnDisable()
	{
		HideDialogue();
	}

	public void ShowDialogue(string title, string body, Action continueCallback, Sprite screenshot = null)
	{
		m_onContinue = continueCallback;

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

		m_onContinue = null;
	}

	private void HandleContinue()
	{
		if (hidePanelOnContinue && panel != null)
		{
			panel.SetActive(false);
		}

		Action callback = m_onContinue;
		m_onContinue = null;
		callback?.Invoke();
	}
}

public class DialogueService : DialogueUiservice, IGraphDialogueService
{
	public UniTask ShowAsync(string title, string body, CancellationToken cancellationToken)
	{
		var completionSource = new UniTaskCompletionSource();
		CancellationTokenRegistration
			registration = cancellationToken.Register(() => completionSource.TrySetCanceled());
		ShowDialogue(title, body, () => completionSource.TrySetResult());
		return AwaitWithCleanupAsync(this, completionSource, registration);
	}

	public void EndConversation()
	{
		HideDialogue();
	}

	private static async UniTask AwaitWithCleanupAsync(
		DialogueUiservice dialogueUiservice,
		UniTaskCompletionSource completionSource,
		CancellationTokenRegistration registration)
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
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Graph.Core.Runtime;
using Graph.Core.Runtime.Nodes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample.Runtime.UI
{
	public class ChoiceUiservice : MonoBehaviour, IGraphChoiceService
	{
		public GameObject panel;
		public Button[] optionButtons = new Button[4];
		public TMP_Text[] optionTexts = new TMP_Text[4];

		private Action<int> m_onSelected;

		private void Awake()
		{
			if (panel != null)
			{
				panel.SetActive(false);
			}
		}

		public UniTask<int> ShowAsync(IReadOnlyList<GraphChoiceEntry> options, CancellationToken cancellationToken)
		{
			var completionSource = new UniTaskCompletionSource<int>();
			CancellationTokenRegistration
				registration = cancellationToken.Register(() => completionSource.TrySetCanceled());
			var legacyOptions = new List<ChoiceOption>();
			if (options != null)
			{
				for (var i = 0; i < options.Count; i++)
				{
					legacyOptions.Add(new ChoiceOption
					{
						label = options[i].label
					});
				}
			}

			ShowChoices(legacyOptions, index => completionSource.TrySetResult(index));
			return AwaitWithCleanupAsync(completionSource, registration);
		}

		private static async UniTask<int> AwaitWithCleanupAsync(
			UniTaskCompletionSource<int> completionSource,
			CancellationTokenRegistration registration)
		{
			try
			{
				return await completionSource.Task;
			}
			finally
			{
				registration.Dispose();
			}
		}

		public void ShowChoices(IReadOnlyList<ChoiceOption> options, Action<int> onSelectedCallback)
		{
			m_onSelected = onSelectedCallback;
			ApplyOptions(options);
		}

		private void ApplyOptions(IReadOnlyList<ChoiceOption> options)
		{
			if (panel != null)
			{
				panel.SetActive(true);
			}

			for (var i = 0; i < 4; i++)
			{
				Button button = optionButtons != null && i < optionButtons.Length ? optionButtons[i] : null;
				TMP_Text label = optionTexts != null && i < optionTexts.Length ? optionTexts[i] : null;

				if (button == null)
				{
					continue;
				}

				button.onClick.RemoveAllListeners();

				ChoiceOption option = options != null && i < options.Count ? options[i] : null;
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

			Action<int> callback = m_onSelected;
			m_onSelected = null;
			callback?.Invoke(index);
		}

		public void HideChoices()
		{
			if (panel != null)
			{
				panel.SetActive(false);
			}

			m_onSelected = null;

			if (optionButtons == null)
			{
				return;
			}

			for (var i = 0; i < optionButtons.Length; i++)
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
}
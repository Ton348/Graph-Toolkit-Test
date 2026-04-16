using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample.Runtime.UI
{
	public class TradeOfferUiservice : MonoBehaviour
	{
		public GameObject panel;
		public TMP_Text titleText;
		public TMP_Text fullPriceText;
		public TMP_Text offerValueText;
		public Slider offerSlider;
		public Button confirmButton;
		private int m_currentFullPrice;

		private Action<int> m_onConfirm;

		public bool IsOpen => panel != null && panel.activeSelf;

		private void Awake()
		{
			if (panel != null)
			{
				panel.SetActive(false);
			}

			if (offerSlider != null)
			{
				offerSlider.onValueChanged.AddListener(value => UpdateOfferLabel());
			}

			if (confirmButton != null)
			{
				confirmButton.onClick.AddListener(HandleConfirm);
			}
		}

		public void ShowOffer(string buildingLabel, int fullPrice, Action<int> confirmCallback)
		{
			m_onConfirm = confirmCallback;
			m_currentFullPrice = Mathf.Max(1, fullPrice);

			if (titleText != null)
			{
				titleText.text = string.IsNullOrEmpty(buildingLabel) ? string.Empty : buildingLabel;
			}

			if (fullPriceText != null)
			{
				fullPriceText.text = $"{m_currentFullPrice}";
			}

			if (offerSlider != null)
			{
				offerSlider.minValue = 1;
				offerSlider.maxValue = m_currentFullPrice;
				offerSlider.wholeNumbers = true;
				offerSlider.value = m_currentFullPrice;
			}

			if (confirmButton != null)
			{
				confirmButton.interactable = true;
			}

			UpdateOfferLabel();

			if (panel != null)
			{
				panel.SetActive(true);
			}
		}

		public void Hide()
		{
			if (panel != null)
			{
				panel.SetActive(false);
			}

			m_onConfirm = null;
		}

		private void UpdateOfferLabel()
		{
			if (offerValueText == null || offerSlider == null)
			{
				return;
			}

			int value = Mathf.RoundToInt(offerSlider.value);
			offerValueText.text = $"{value}";
		}

		private void HandleConfirm()
		{
			if (confirmButton != null)
			{
				confirmButton.interactable = false;
			}

			int amount = offerSlider != null ? Mathf.RoundToInt(offerSlider.value) : m_currentFullPrice;
			Action<int> callback = m_onConfirm;
			m_onConfirm = null;
			Hide();
			callback?.Invoke(amount);
		}
	}
}
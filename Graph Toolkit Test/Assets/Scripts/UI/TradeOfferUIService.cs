using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeOfferUIService : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text fullPriceText;
    public TMP_Text offerValueText;
    public Slider offerSlider;
    public Button confirmButton;

    private Action<int> onConfirm;
    private int currentFullPrice;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (offerSlider != null)
        {
            offerSlider.onValueChanged.AddListener(_ => UpdateOfferLabel());
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HandleConfirm);
        }
    }

    public void ShowOffer(string buildingLabel, int fullPrice, Action<int> confirmCallback)
    {
        onConfirm = confirmCallback;
        currentFullPrice = Mathf.Max(1, fullPrice);

        if (titleText != null)
        {
            titleText.text = string.IsNullOrEmpty(buildingLabel) ? string.Empty : buildingLabel;
        }

        if (fullPriceText != null)
        {
            fullPriceText.text = $"{currentFullPrice}";
        }

        if (offerSlider != null)
        {
            offerSlider.minValue = 1;
            offerSlider.maxValue = currentFullPrice;
            offerSlider.wholeNumbers = true;
            offerSlider.value = currentFullPrice;
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

        onConfirm = null;
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

        int amount = offerSlider != null ? Mathf.RoundToInt(offerSlider.value) : currentFullPrice;
        var callback = onConfirm;
        onConfirm = null;
        Hide();
        callback?.Invoke(amount);
    }
}

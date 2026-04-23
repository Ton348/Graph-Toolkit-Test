using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample.Runtime.UI
{
	public sealed class TraderShopItemData
	{
		public string itemId;
		public string category;
		public string name;
		public string description;
		public int price;
		public int storageCapacity;
		public int cashCapacity;
		public int shelfCapacity;
		public bool isOwned;
	}

	public sealed class TraderShopUIService : MonoBehaviour
	{
		public GameObject panel;
		public TMP_Text titleText;
		public RectTransform itemsRoot;
		public Button itemButtonPrefab;

		private Action<string> m_onConfirm;
		private Action m_onCancel;
		private Button m_closeButton;
		private Transform m_itemTemplateRoot;
		private GameObject m_dimmer;

		public bool IsOpen => panel != null && panel.activeSelf;

		private void Awake()
		{
			ResolveReferences();

			if (panel != null)
			{
				panel.SetActive(false);
			}

			if (m_dimmer != null)
			{
				m_dimmer.SetActive(false);
			}

			if (itemButtonPrefab != null)
			{
				itemButtonPrefab.gameObject.SetActive(false);
			}

			if (m_itemTemplateRoot != null)
			{
				m_itemTemplateRoot.gameObject.SetActive(false);
			}

			m_closeButton = FindCloseButton();
			if (m_closeButton != null)
			{
				m_closeButton.onClick.RemoveListener(HandleCancel);
				m_closeButton.onClick.AddListener(HandleCancel);
			}
		}

		private void ResolveReferences()
		{
			if (panel == null)
			{
				Transform panelTransform = transform.Find("Panel");
				if (panelTransform != null)
				{
					panel = panelTransform.gameObject;
				}
			}

			if (m_dimmer == null)
			{
				Transform dimmerTransform = transform.Find("Dimmer");
				if (dimmerTransform != null)
				{
					m_dimmer = dimmerTransform.gameObject;
				}
			}

			if (titleText == null)
			{
				Transform titleTransform = transform.Find("Panel/Header/Left/TitleText");
				if (titleTransform != null)
				{
					titleText = titleTransform.GetComponent<TMP_Text>();
				}
			}

			if (itemsRoot == null)
			{
				Transform itemsRootTransform = transform.Find("Panel/Body/ItemListScrollView/Viewport/Content");
				if (itemsRootTransform != null)
				{
					itemsRoot = itemsRootTransform as RectTransform;
				}
			}

			if (itemButtonPrefab == null && itemsRoot != null)
			{
				Transform buyButtonTransform = itemsRoot.Find("ItemCardTemplate/BottomRow/BuyButton");
				if (buyButtonTransform != null)
				{
					itemButtonPrefab = buyButtonTransform.GetComponent<Button>();
				}
			}

			m_itemTemplateRoot = ResolveTemplateRoot();
		}

		public void ShowTrader(
			string traderName,
			int money,
			IReadOnlyList<TraderShopItemData> items,
			Action<string> confirmCallback,
			Action cancelCallback)
		{
			m_onConfirm = confirmCallback;
			m_onCancel = cancelCallback;

			ResolveReferences();

			if (titleText != null)
			{
				titleText.text = !string.IsNullOrWhiteSpace(traderName)
					? traderName
					: "Магазин торговца";
			}

			RebuildItems(items, money);

			gameObject.SetActive(true);

			if (m_dimmer != null)
			{
				m_dimmer.SetActive(true);
			}

			if (panel != null)
			{
				panel.SetActive(true);
			}
		}

		public void Hide()
		{
			if (m_dimmer != null)
			{
				m_dimmer.SetActive(false);
			}

			if (panel != null)
			{
				panel.SetActive(false);
			}

			ClearSpawnedItems();
			m_onConfirm = null;
			m_onCancel = null;
		}

		private void HandleConfirm()
		{
			Action<string> callback = m_onConfirm;
			Hide();
			callback?.Invoke(string.Empty);
		}

		private void HandleCancel()
		{
			Action callback = m_onCancel;
			Hide();
			callback?.Invoke();
		}

		private void RebuildItems(IReadOnlyList<TraderShopItemData> items, int money)
		{
			ClearSpawnedItems();
			ResolveReferences();
			if (items == null || itemsRoot == null)
			{
				return;
			}

			if (m_itemTemplateRoot != null)
			{
				for (int i = 0; i < items.Count; i++)
				{
					TraderShopItemData item = items[i];
					if (item == null)
					{
						continue;
					}

					GameObject cardObject = Instantiate(m_itemTemplateRoot.gameObject, itemsRoot);
					cardObject.name = $"ItemCard_{i}";
					cardObject.SetActive(true);

					RectTransform cardRect = cardObject.transform as RectTransform;
					if (cardRect != null)
					{
						cardRect.localScale = Vector3.one;
					}

					TMP_Text itemNameText = FindText(cardObject.transform, "ItemNameText");
					if (itemNameText != null)
					{
						itemNameText.text = string.IsNullOrWhiteSpace(item.name) ? "Товар" : item.name;
					}

					TMP_Text itemDescriptionText = FindText(cardObject.transform, "ItemDescriptionText");
					if (itemDescriptionText != null)
					{
						itemDescriptionText.text = string.IsNullOrWhiteSpace(item.description) ? "Без описания" : item.description;
					}

					TMP_Text itemStatsText = FindText(cardObject.transform, "ItemStatsText");
					if (itemStatsText != null)
					{
						itemStatsText.text = BuildStatsText(item);
					}

					TMP_Text priceText = FindText(cardObject.transform, "BottomRow/PriceText");
					string state = item.isOwned ? "Уже куплено" : money < item.price ? "Недостаточно денег" : "Купить";
					if (priceText != null)
					{
						priceText.text = $"Цена: {item.price}$";
					}

					Button buyButton = FindButton(cardObject.transform, "BuyButton");
					if (buyButton != null)
					{
						buyButton.gameObject.SetActive(true);
						buyButton.onClick.RemoveAllListeners();
						string itemId = item.itemId;
						buyButton.onClick.AddListener(() => HandleItemSelected(itemId));
						buyButton.interactable = !item.isOwned && money >= item.price;

						TMP_Text buyButtonLabel = buyButton.GetComponentInChildren<TMP_Text>(true);
						if (buyButtonLabel != null)
						{
							buyButtonLabel.text = state;
						}
					}
				}

				LayoutRebuilder.ForceRebuildLayoutImmediate(itemsRoot);
				return;
			}

			if (itemButtonPrefab == null)
			{
				return;
			}

			for (int i = 0; i < items.Count; i++)
			{
				TraderShopItemData item = items[i];
				if (item == null)
				{
					continue;
				}

				Button button = Instantiate(itemButtonPrefab, itemsRoot);
				button.gameObject.SetActive(true);
				button.onClick.RemoveAllListeners();
				string itemId = item.itemId;
				button.onClick.AddListener(() => HandleItemSelected(itemId));
				button.interactable = !item.isOwned && money >= item.price;

				TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
				if (label != null)
				{
					string state = item.isOwned ? "Уже куплено" : money < item.price ? "Недостаточно денег" : "Купить";
					string details = !string.IsNullOrWhiteSpace(item.description) ? $"\n{item.description}" : string.Empty;
					label.text = $"{item.name}  {item.price}$  [{state}]" + details;
				}
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(itemsRoot);
		}

		private void ClearSpawnedItems()
		{
			if (itemsRoot == null)
			{
				return;
			}

			for (int i = itemsRoot.childCount - 1; i >= 0; i--)
			{
				Transform child = itemsRoot.GetChild(i);
				if (itemButtonPrefab != null && child == itemButtonPrefab.transform)
				{
					continue;
				}

				if (m_itemTemplateRoot != null && child == m_itemTemplateRoot)
				{
					continue;
				}

				Destroy(child.gameObject);
			}
		}

		private Transform ResolveTemplateRoot()
		{
			if (itemButtonPrefab == null)
			{
				return null;
			}

			Transform current = itemButtonPrefab.transform;
			while (current != null && current.parent != itemsRoot)
			{
				current = current.parent;
			}

			return current;
		}

		private static TMP_Text FindText(Transform root, string childName)
		{
			if (root == null)
			{
				return null;
			}

			Transform child = root.Find(childName);
			return child != null ? child.GetComponent<TMP_Text>() : null;
		}

		private static Button FindButton(Transform root, string childName)
		{
			if (root == null)
			{
				return null;
			}

			Transform child = root.Find("BottomRow/" + childName);
			return child != null ? child.GetComponent<Button>() : null;
		}

		private static string BuildStatsText(TraderShopItemData item)
		{
			if (item == null)
			{
				return "Характеристики не указаны";
			}

			List<string> lines = new();
			string categoryLabel = GetCategoryLabel(item.category);
			if (!string.IsNullOrWhiteSpace(categoryLabel))
			{
				lines.Add($"Категория: {categoryLabel}");
			}

			if (item.storageCapacity > 0)
			{
				lines.Add($"Вместимость склада: {item.storageCapacity}");
			}

			if (item.cashCapacity > 0)
			{
				lines.Add($"Лимит кассы: {item.cashCapacity}");
			}

			if (item.shelfCapacity > 0)
			{
				lines.Add($"Вместимость полок: {item.shelfCapacity}");
			}

			return lines.Count > 0 ? string.Join("\n", lines) : "Характеристики не указаны";
		}

		private static string GetCategoryLabel(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				return null;
			}

			return category switch
			{
				"storage" => "Склад",
				"storage2" => "Склад",
				"cashdesk" => "Касса",
				"cashdesk2" => "Касса",
				"shelf" => "Полки",
				"shelf2" => "Полки",
				_ => category
			};
		}

		private void HandleItemSelected(string itemId)
		{
			Action<string> callback = m_onConfirm;
			Hide();
			callback?.Invoke(itemId);
		}

		private Button FindCloseButton()
		{
			Transform closeTransform = transform.Find("Panel/Header/CloseButton");
			if (closeTransform != null)
			{
				Button closeButton = closeTransform.GetComponent<Button>();
				if (closeButton != null)
				{
					return closeButton;
				}
			}

			Button[] buttons = GetComponentsInChildren<Button>(true);
			for (int i = 0; i < buttons.Length; i++)
			{
				Button button = buttons[i];
				if (button == null || button == itemButtonPrefab)
				{
					continue;
				}

				TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
				if (text != null && string.Equals(text.text, "X", StringComparison.OrdinalIgnoreCase))
				{
					return button;
				}
			}

			return null;
		}
	}
}

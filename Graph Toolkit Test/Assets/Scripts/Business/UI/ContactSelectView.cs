using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Prototype.Business.UI
{
	[Serializable]
	public sealed class ContactSelectOption
	{
		public string id;
		public string displayName;
	}

	public sealed class ContactSelectView : MonoBehaviour
	{
		private static readonly List<ContactSelectView> s_instances = new();

		[SerializeField]
		private TMP_Text selectedNameText;

		[SerializeField]
		private Button changeButton;

		[SerializeField]
		private GameObject popupRoot;

		[SerializeField]
		private Transform itemsRoot;

		[SerializeField]
		private Button itemButtonPrefab;

		[SerializeField]
		private int popupSortingOrder = 500;

		private readonly List<ContactSelectOption> m_options = new();
		private readonly List<Button> m_spawnedButtons = new();
		private string m_selectedId;
		private Transform m_liftedContainer;
		private int m_liftedContainerSiblingIndex = -1;
		private Canvas m_popupCanvas;
		private GraphicRaycaster m_popupRaycaster;

		public event Action<string> selectionChanged;

		private void Awake()
		{
			if (!s_instances.Contains(this))
			{
				s_instances.Add(this);
			}

			if (changeButton != null)
			{
				changeButton.onClick.AddListener(TogglePopup);
			}

			if (itemButtonPrefab != null)
			{
				itemButtonPrefab.gameObject.SetActive(false);
				EnsureButtonReceivesClicks(itemButtonPrefab);
				SanitizeTextMapping(itemButtonPrefab.transform);
				EnsureOnlyButtonsBlockRaycasts(itemButtonPrefab.transform);
			}

			if (popupRoot != null)
			{
				EnsurePopupCanvas();
				popupRoot.SetActive(false);
				SanitizeTextMapping(popupRoot.transform);
				EnsureOnlyButtonsBlockRaycasts(popupRoot.transform);
			}
		}

		private void OnDestroy()
		{
			RestoreContainerOrder();
			s_instances.Remove(this);
		}

		public void Setup(IEnumerable<ContactSelectOption> options, string selectedId, bool includeNone = true, string noneLabel = "Нет")
		{
			m_options.Clear();
			string normalizedSelectedId = NormalizeId(selectedId);

			if (includeNone)
			{
				m_options.Add(new ContactSelectOption
				{
					id = string.Empty,
					displayName = noneLabel
				});
			}

			if (options != null)
			{
				foreach (ContactSelectOption option in options)
				{
					if (option == null)
					{
						continue;
					}

					m_options.Add(new ContactSelectOption
					{
						id = option.id,
						displayName = option.displayName
					});
				}
			}

			if (!string.IsNullOrWhiteSpace(normalizedSelectedId))
			{
				bool hasSelected = false;
				for (int i = 0; i < m_options.Count; i++)
				{
					ContactSelectOption candidate = m_options[i];
					if (candidate != null && NormalizeId(candidate.id) == normalizedSelectedId)
					{
						hasSelected = true;
						break;
					}
				}

				if (!hasSelected)
				{
					m_options.Add(new ContactSelectOption
					{
						id = normalizedSelectedId,
						displayName = normalizedSelectedId
					});
				}
			}

			RebuildItems();
			SetSelected(normalizedSelectedId, false);
		}

		public string GetSelectedId()
		{
			return NormalizeId(m_selectedId);
		}

		private void TogglePopup()
		{
			if (popupRoot == null)
			{
				return;
			}

			EnsurePopupCanvas();
			bool willOpen = !popupRoot.activeSelf;
			if (willOpen)
			{
				CloseAllExceptThis();
				LiftContainerForPopup();
			}

			popupRoot.SetActive(!popupRoot.activeSelf);
			if (popupRoot.activeSelf)
			{
				popupRoot.transform.SetAsLastSibling();
			}
			else
			{
				RestoreContainerOrder();
			}
		}

		private void RebuildItems()
		{
			for (int i = 0; i < m_spawnedButtons.Count; i++)
			{
				Button button = m_spawnedButtons[i];
				if (button != null)
				{
					Destroy(button.gameObject);
				}
			}

			m_spawnedButtons.Clear();

			if (itemsRoot == null || itemButtonPrefab == null)
			{
				return;
			}

			itemButtonPrefab.gameObject.SetActive(false);

			for (int i = 0; i < m_options.Count; i++)
			{
				ContactSelectOption option = m_options[i];
				Button button = Instantiate(itemButtonPrefab, itemsRoot, false);
				button.gameObject.SetActive(true);
				button.interactable = true;
				button.enabled = true;

				RectTransform buttonRect = button.transform as RectTransform;
				if (buttonRect != null)
				{
					buttonRect.anchorMin = new Vector2(0f, 1f);
					buttonRect.anchorMax = new Vector2(1f, 1f);
					buttonRect.pivot = new Vector2(0.5f, 0.5f);
					buttonRect.anchoredPosition = Vector2.zero;
					buttonRect.localScale = Vector3.one;
				}

				button.name = $"Option_{i}";
				button.onClick.RemoveAllListeners();
				string optionId = option.id;
				button.onClick.AddListener(() => SetSelected(optionId, true));

				TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
				if (label != null)
				{
					string labelValue = string.IsNullOrWhiteSpace(option.displayName) ? option.id : option.displayName;
					label.text = string.IsNullOrWhiteSpace(labelValue) ? "Нет" : labelValue;
					label.enableWordWrapping = false;
					label.overflowMode = TextOverflowModes.Ellipsis;
					label.horizontalMapping = TextureMappingOptions.Character;
					label.verticalMapping = TextureMappingOptions.Character;
					label.alignment = TextAlignmentOptions.MidlineLeft;
					SanitizeLabelMapping(label);
				}

				EnsureButtonReceivesClicks(button);
				SanitizeTextMapping(button.transform);
				EnsureOnlyButtonsBlockRaycasts(button.transform);

				RectTransform itemsRect = itemsRoot as RectTransform;
				if (itemsRect != null)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(itemsRect);
				}

				m_spawnedButtons.Add(button);
			}
		}

		private void SetSelected(string id, bool notify)
		{
			string normalized = NormalizeId(id);
			ContactSelectOption selected = null;
			for (int i = 0; i < m_options.Count; i++)
			{
				ContactSelectOption candidate = m_options[i];
				if (NormalizeId(candidate.id) == normalized)
				{
					selected = candidate;
					break;
				}
			}

			if (selected == null && m_options.Count > 0)
			{
				selected = m_options[0];
				normalized = NormalizeId(selected.id);
			}

			m_selectedId = normalized;

			if (selectedNameText != null)
			{
				string text = selected != null ? selected.displayName : null;
				selectedNameText.text = string.IsNullOrWhiteSpace(text) ? "Нет" : text;
			}

			if (notify && popupRoot != null)
			{
				popupRoot.SetActive(false);
				RestoreContainerOrder();
			}

			if (notify)
			{
				selectionChanged?.Invoke(m_selectedId);
			}
		}

		private static string NormalizeId(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			return value.Trim();
		}

		private void CloseAllExceptThis()
		{
			for (int i = 0; i < s_instances.Count; i++)
			{
				ContactSelectView view = s_instances[i];
				if (view == null || view == this || view.popupRoot == null)
				{
					continue;
				}

				view.popupRoot.SetActive(false);
				view.RestoreContainerOrder();
			}
		}

		private void EnsurePopupCanvas()
		{
			if (popupRoot == null)
			{
				return;
			}

			if (m_popupCanvas == null)
			{
				m_popupCanvas = popupRoot.GetComponent<Canvas>();
				if (m_popupCanvas == null)
				{
					m_popupCanvas = popupRoot.AddComponent<Canvas>();
				}
			}

			m_popupCanvas.overrideSorting = true;
			m_popupCanvas.sortingOrder = popupSortingOrder;

			if (m_popupRaycaster == null)
			{
				m_popupRaycaster = popupRoot.GetComponent<GraphicRaycaster>();
				if (m_popupRaycaster == null)
				{
					m_popupRaycaster = popupRoot.AddComponent<GraphicRaycaster>();
				}
			}
		}

		private void LiftContainerForPopup()
		{
			if (m_liftedContainer != null)
			{
				return;
			}

			Transform candidate = transform.parent;
			if (candidate == null || candidate.parent == null)
			{
				return;
			}

			m_liftedContainer = candidate;
			m_liftedContainerSiblingIndex = candidate.GetSiblingIndex();
			candidate.SetAsLastSibling();
		}

		private void RestoreContainerOrder()
		{
			if (m_liftedContainer == null || m_liftedContainer.parent == null)
			{
				m_liftedContainer = null;
				m_liftedContainerSiblingIndex = -1;
				return;
			}

			int maxIndex = m_liftedContainer.parent.childCount - 1;
			int safeIndex = Mathf.Clamp(m_liftedContainerSiblingIndex, 0, maxIndex);
			m_liftedContainer.SetSiblingIndex(safeIndex);
			m_liftedContainer = null;
			m_liftedContainerSiblingIndex = -1;
		}

		private static void SanitizeTextMapping(Transform root)
		{
			if (root == null)
			{
				return;
			}

			TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
			for (int i = 0; i < labels.Length; i++)
			{
				SanitizeLabelMapping(labels[i]);
			}
		}

		private static void SanitizeLabelMapping(TMP_Text label)
		{
			if (label == null)
			{
				return;
			}

			label.horizontalMapping = TextureMappingOptions.Character;
			label.verticalMapping = TextureMappingOptions.Character;
			label.raycastTarget = false;
		}

		private static void EnsureOnlyButtonsBlockRaycasts(Transform root)
		{
			if (root == null)
			{
				return;
			}

			Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < graphics.Length; i++)
			{
				Graphic graphic = graphics[i];
				if (graphic == null)
				{
					continue;
				}

				Button ownerButton = graphic.GetComponent<Button>();
				if (ownerButton != null && graphic.transform == ownerButton.transform)
				{
					graphic.raycastTarget = true;
					continue;
				}

				if (graphic.GetComponentInParent<Button>() == null)
				{
					graphic.raycastTarget = false;
				}
			}
		}

		private static void EnsureButtonReceivesClicks(Button button)
		{
			if (button == null)
			{
				return;
			}

			Image buttonImage = button.GetComponent<Image>();
			if (buttonImage != null)
			{
				button.targetGraphic = buttonImage;
				buttonImage.raycastTarget = true;
			}

			Graphic[] childGraphics = button.GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < childGraphics.Length; i++)
			{
				Graphic graphic = childGraphics[i];
				if (graphic == null || graphic.transform == button.transform)
				{
					continue;
				}

				graphic.raycastTarget = false;
			}
		}

	}
}

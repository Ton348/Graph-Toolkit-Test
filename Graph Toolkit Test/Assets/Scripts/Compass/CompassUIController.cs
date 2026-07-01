using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sample.Runtime.Compass
{
	public class CompassUIController : MonoBehaviour
	{
		private const int TickCount = 36;
		private const float TickStep = 10f;

		[Header("References")]
		[SerializeField]
		private CompassManager m_compassManager;

		[SerializeField]
		private RectTransform m_compassBar;

		[SerializeField]
		private Transform m_markersContainer;

		[SerializeField]
		private CompassMarkerView m_markerPrefab;

		[SerializeField]
		private Transform m_ticksContainer;

		[SerializeField]
		private CompassTickView m_tickPrefab;

		[Header("Settings")]
		[SerializeField]
		private float m_maxVisibleAngle = 90f;

		private readonly HashSet<string> m_activeIds = new();
		private readonly Dictionary<string, CompassMarkerView> m_markers = new();
		private readonly List<TickRuntime> m_ticks = new();
		private readonly List<string> m_toRemove = new();
		private readonly List<CardinalLabelRuntime> m_cardinalLabels = new();

		private float m_halfWidth;

		private void Awake()
		{
			if (m_compassManager == null)
			{
				m_compassManager = CompassManager.Instance;
			}

			if (m_compassBar == null)
			{
				m_compassBar = GetComponent<RectTransform>();
			}

			if (m_markersContainer == null && m_compassBar != null)
			{
				m_markersContainer = m_compassBar;
			}

			if (m_ticksContainer == null && m_compassBar != null)
			{
				m_ticksContainer = m_compassBar;
			}

			if (m_ticksContainer == null)
			{
				m_ticksContainer = transform;
			}

			CacheCardinalLabels();
			EnsureTicks();
			CacheHalfWidth();
		}

		private void OnEnable()
		{
			if (m_compassManager != null)
			{
				m_compassManager.activeTargetsChanged += SyncMarkers;
			}

			SyncMarkers();
		}

		private void OnDisable()
		{
			if (m_compassManager != null)
			{
				m_compassManager.activeTargetsChanged -= SyncMarkers;
			}
		}

		private void LateUpdate()
		{
			UpdateMarkers();
			UpdateTicks();
			UpdateCardinals();
		}

		private void OnRectTransformDimensionsChange()
		{
			CacheHalfWidth();
		}

		private void CacheHalfWidth()
		{
			if (m_compassBar != null)
			{
				m_halfWidth = m_compassBar.rect.width * 0.5f;
			}
		}

		private void CacheCardinalLabels()
		{
			m_cardinalLabels.Clear();

			if (m_compassBar == null)
			{
				return;
			}

			RectTransform[] children = m_compassBar.GetComponentsInChildren<RectTransform>(true);
			for (int i = 0; i < children.Length; i++)
			{
				RectTransform child = children[i];
				if (child == null || child == m_compassBar)
				{
					continue;
				}

				string name = child.name;
				if (string.Equals(name, "N", StringComparison.OrdinalIgnoreCase))
				{
					m_cardinalLabels.Add(new CardinalLabelRuntime { view = child, worldYaw = 0f });
				}
				else if (string.Equals(name, "E", StringComparison.OrdinalIgnoreCase))
				{
					m_cardinalLabels.Add(new CardinalLabelRuntime { view = child, worldYaw = 90f });
				}
				else if (string.Equals(name, "S", StringComparison.OrdinalIgnoreCase))
				{
					m_cardinalLabels.Add(new CardinalLabelRuntime { view = child, worldYaw = 180f });
				}
				else if (string.Equals(name, "W", StringComparison.OrdinalIgnoreCase))
				{
					m_cardinalLabels.Add(new CardinalLabelRuntime { view = child, worldYaw = -90f });
				}
				else
				{
					continue;
				}

				if (m_cardinalLabels.Count > 1)
				{
					CardinalLabelRuntime last = m_cardinalLabels[m_cardinalLabels.Count - 1];
					if (last.view == null)
					{
						m_cardinalLabels.RemoveAt(m_cardinalLabels.Count - 1);
					}
				}
			}
		}

		private void SyncMarkers()
		{
			if (m_compassManager == null || m_markerPrefab == null)
			{
				return;
			}

			Transform markerParent = GetSceneContainer(m_markersContainer);

			m_activeIds.Clear();

			foreach (CompassTarget target in m_compassManager.ActiveTargets)
			{
				if (target == null)
				{
					continue;
				}

				string id = target.TargetId;
				if (string.IsNullOrWhiteSpace(id))
				{
					continue;
				}

				m_activeIds.Add(id);

				if (!m_markers.TryGetValue(id, out CompassMarkerView view) || view == null)
				{
					CompassMarkerView instance = Instantiate(m_markerPrefab);
					if (markerParent != null)
					{
						instance.transform.SetParent(markerParent, false);
					}

					m_markers[id] = instance;
				}
			}

			m_toRemove.Clear();
			foreach (KeyValuePair<string, CompassMarkerView> kvp in m_markers)
			{
				if (!m_activeIds.Contains(kvp.Key))
				{
					m_toRemove.Add(kvp.Key);
				}
			}

			for (int i = 0; i < m_toRemove.Count; i++)
			{
				string id = m_toRemove[i];
				if (m_markers.TryGetValue(id, out CompassMarkerView view) && view != null)
				{
					Destroy(view.gameObject);
				}

				m_markers.Remove(id);
			}
		}

		private void EnsureTicks()
		{
			if (m_tickPrefab == null || m_ticksContainer == null || m_ticks.Count > 0)
			{
				return;
			}

			Transform tickParent = GetSceneContainer(m_ticksContainer);

			for (int i = 0; i < TickCount; i++)
			{
				CompassTickView instance = Instantiate(m_tickPrefab);
				if (tickParent != null)
				{
					instance.transform.SetParent(tickParent, false);
				}

				m_ticks.Add(new TickRuntime
				{
					worldYaw = i * TickStep,
					view = instance
				});
			}
		}

		private void UpdateMarkers()
		{
			if (m_compassManager == null || m_compassManager.Player == null)
			{
				return;
			}

			Vector3 playerPos = m_compassManager.Player.position;
			Vector3 playerForward = m_compassManager.Player.forward;
			playerForward.y = 0f;
			if (playerForward.sqrMagnitude < 0.0001f)
			{
				playerForward = Vector3.forward;
			}

			foreach (CompassTarget target in m_compassManager.ActiveTargets)
			{
				if (target == null)
				{
					continue;
				}

				string id = target.TargetId;
				if (!m_markers.TryGetValue(id, out CompassMarkerView view) || view == null)
				{
					continue;
				}

				Vector3 direction = target.GetMarkerWorldPosition() - playerPos;
				direction.y = 0f;

				if (direction.sqrMagnitude < 0.0001f)
				{
					view.SetVisible(true);
					view.SetPositionX(0f);
					continue;
				}

				float angle = Vector3.SignedAngle(playerForward, direction, Vector3.up);
				float normalized = Mathf.Abs(angle) > m_maxVisibleAngle
					? Mathf.Sign(angle)
					: Mathf.Clamp(angle / m_maxVisibleAngle, -1f, 1f);

				view.SetVisible(true);
				view.SetPositionX(normalized * m_halfWidth);
			}
		}

		private void UpdateTicks()
		{
			if (m_compassManager == null || m_compassManager.Player == null || m_ticks.Count == 0)
			{
				return;
			}

			Vector3 forward = m_compassManager.Player.forward;
			forward.y = 0f;
			if (forward.sqrMagnitude < 0.0001f)
			{
				forward = Vector3.forward;
			}

			float playerYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

			for (int i = 0; i < m_ticks.Count; i++)
			{
				TickRuntime tick = m_ticks[i];
				if (tick.view == null)
				{
					continue;
				}

				float angle = Mathf.DeltaAngle(playerYaw, tick.worldYaw);
				if (Mathf.Abs(angle) > m_maxVisibleAngle)
				{
					tick.view.SetVisible(false);
					continue;
				}

				tick.view.SetVisible(true);
				tick.view.SetPositionX(Mathf.Clamp(angle / m_maxVisibleAngle, -1f, 1f) * m_halfWidth);
			}
		}

		private void UpdateCardinals()
		{
			if (m_compassManager == null || m_compassManager.Player == null || m_cardinalLabels.Count == 0)
			{
				return;
			}

			Vector3 forward = m_compassManager.Player.forward;
			forward.y = 0f;
			if (forward.sqrMagnitude < 0.0001f)
			{
				forward = Vector3.forward;
			}

			float playerYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
			float cardinalStep = 90f;

			for (int i = 0; i < m_cardinalLabels.Count; i++)
			{
				RectTransform label = m_cardinalLabels[i].view;
				if (label == null)
				{
					continue;
				}

				float angle = Mathf.DeltaAngle(playerYaw, m_cardinalLabels[i].worldYaw);
				float normalized = Mathf.Clamp(angle / m_maxVisibleAngle, -1f, 1f);

				Vector2 pos = label.anchoredPosition;
				pos.x = normalized * m_halfWidth;
				label.anchoredPosition = pos;
			}
		}

		private class TickRuntime
		{
			public float worldYaw;
			public CompassTickView view;
		}

		private class CardinalLabelRuntime
		{
			public RectTransform view;
			public float worldYaw;
		}

		private static Transform GetSceneContainer(Transform container)
		{
			if (container != null && container.gameObject.scene.IsValid())
			{
				return container;
			}

			return null;
		}
	}
}

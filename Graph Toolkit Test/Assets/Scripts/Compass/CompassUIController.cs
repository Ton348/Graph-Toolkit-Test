using System;
using System.Collections.Generic;
using UnityEngine;

public class CompassUicontroller : MonoBehaviour
{
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

	[SerializeField]
	private List<TickDefinition> m_tickDefinitions = new();

	private readonly HashSet<string> m_activeIds = new();

	private readonly Dictionary<string, CompassMarkerView> m_markers = new();
	private readonly List<TickRuntime> m_ticks = new();
	private readonly List<string> m_toRemove = new();

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

		if (!IsSceneTransform(m_ticksContainer))
		{
			m_ticksContainer = IsSceneTransform(m_compassBar) ? m_compassBar : transform;
		}

		EnsureDefaultTicks();
		CacheHalfWidth();
	}

	private void LateUpdate()
	{
		UpdateMarkers();
		UpdateTicks();
	}

	private void OnEnable()
	{
		if (m_compassManager != null)
		{
			m_compassManager.activeTargetsChanged += SyncMarkers;
		}

		EnsureTicks();
		SyncMarkers();
	}

	private void OnDisable()
	{
		if (m_compassManager != null)
		{
			m_compassManager.activeTargetsChanged -= SyncMarkers;
		}
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

	private void SyncMarkers()
	{
		if (m_compassManager == null || m_markerPrefab == null)
		{
			return;
		}

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
				CompassMarkerView instance = Instantiate(m_markerPrefab, m_markersContainer);
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

		for (var i = 0; i < m_toRemove.Count; i++)
		{
			string id = m_toRemove[i];
			if (m_markers.TryGetValue(id, out CompassMarkerView view) && view != null)
			{
				Destroy(view.gameObject);
			}

			m_markers.Remove(id);
		}
	}

	private void EnsureDefaultTicks()
	{
		if (m_tickDefinitions != null && m_tickDefinitions.Count > 0)
		{
			return;
		}

		m_tickDefinitions = new List<TickDefinition>
		{
			new() { label = "N", worldYaw = 0f },
			new() { label = "E", worldYaw = 90f },
			new() { label = "S", worldYaw = 180f },
			new() { label = "W", worldYaw = -90f }
		};
	}

	private void EnsureTicks()
	{
		if (m_tickPrefab == null || m_ticksContainer == null)
		{
			return;
		}

		if (m_ticks.Count > 0)
		{
			return;
		}

		if (!IsSceneTransform(m_ticksContainer))
		{
			m_ticksContainer = IsSceneTransform(m_compassBar) ? m_compassBar : transform;
		}

		EnsureDefaultTicks();

		for (var i = 0; i < m_tickDefinitions.Count; i++)
		{
			CompassTickView instance = Instantiate(m_tickPrefab, m_ticksContainer);
			instance.SetLabel(m_tickDefinitions[i].label);
			m_ticks.Add(new TickRuntime { def = m_tickDefinitions[i], view = instance });
		}
	}

	private static bool IsSceneTransform(Transform t)
	{
		return t != null && t.gameObject.scene.IsValid();
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

			float normalized;
			if (Mathf.Abs(angle) > m_maxVisibleAngle)
			{
				normalized = Mathf.Sign(angle);
			}
			else
			{
				normalized = Mathf.Clamp(angle / m_maxVisibleAngle, -1f, 1f);
			}

			float x = normalized * m_halfWidth;

			view.SetVisible(true);
			view.SetPositionX(x);
		}
	}

	private void UpdateTicks()
	{
		if (m_compassManager == null || m_compassManager.Player == null)
		{
			return;
		}

		if (m_ticks.Count == 0)
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

		for (var i = 0; i < m_ticks.Count; i++)
		{
			TickRuntime tick = m_ticks[i];
			if (tick.view == null)
			{
				continue;
			}

			float angle = Mathf.DeltaAngle(playerYaw, tick.def.worldYaw);
			if (Mathf.Abs(angle) > m_maxVisibleAngle)
			{
				tick.view.SetVisible(false);
				continue;
			}

			float normalized = Mathf.Clamp(angle / m_maxVisibleAngle, -1f, 1f);
			float x = normalized * m_halfWidth;

			tick.view.SetVisible(true);
			tick.view.SetPositionX(x);
		}
	}

	[Serializable]
	public struct TickDefinition
	{
		public string label;

		[Range(-180f, 180f)]
		public float worldYaw;
	}

	private class TickRuntime
	{
		public TickDefinition def;
		public CompassTickView view;
	}
}
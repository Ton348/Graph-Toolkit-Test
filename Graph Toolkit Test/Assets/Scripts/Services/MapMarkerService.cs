using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapMarkerService : MonoBehaviour
{
	public GameObject markerPrefab;
	public List<MarkerTarget> markerTargets = new();

	private readonly Dictionary<string, GameObject> m_activeMarkers = new();

	public void ShowMarker(string markerId, Transform targetTransform, string title)
	{
		Transform target = targetTransform != null ? targetTransform : GetTarget(markerId);
		if (target == null || markerPrefab == null)
		{
			return;
		}

		if (m_activeMarkers.ContainsKey(markerId))
		{
			return;
		}

		GameObject marker = Instantiate(markerPrefab, target.position, Quaternion.identity, target);
		SetMarkerTitle(marker, title);
		m_activeMarkers[markerId] = marker;
	}

	public void HideMarker(string markerId)
	{
		if (!m_activeMarkers.TryGetValue(markerId, out GameObject marker))
		{
			return;
		}

		if (marker != null)
		{
			Destroy(marker);
		}

		m_activeMarkers.Remove(markerId);
	}

	public Transform GetTarget(string markerId)
	{
		if (string.IsNullOrEmpty(markerId))
		{
			return null;
		}

		foreach (MarkerTarget mt in markerTargets)
		{
			if (mt != null && mt.markerId == markerId)
			{
				return mt.target;
			}
		}

		return null;
	}

	private void SetMarkerTitle(GameObject marker, string title)
	{
		if (marker == null)
		{
			return;
		}

		var label = marker.GetComponentInChildren<TMP_Text>();
		if (label != null)
		{
			label.text = title;
		}
	}

	[Serializable]
	public class MarkerTarget
	{
		public string markerId;
		public Transform target;
	}
}
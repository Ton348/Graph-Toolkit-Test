using Prototype.Business.NPC.AI;
using TMPro;
using UnityEngine;

namespace Prototype.Business.NPC.Debug
{
	public sealed class NPCDebugManager : MonoBehaviour
	{
		[SerializeField] private float cullDistance = 30f;
		[SerializeField] private float updateInterval = 0.25f;

		private float m_timer;
		private Camera m_camera;
		private NPCBrain[] m_cachedBrains;

		private void Awake()
		{
			m_camera = Camera.main;
		}

		private void Update()
		{
			m_timer += Time.deltaTime;
			if (m_timer < Mathf.Max(0.05f, updateInterval))
			{
				return;
			}

			m_timer = 0f;
			RefreshAllLabels();
		}

		private void RefreshAllLabels()
		{
			if (m_camera == null)
			{
				m_camera = Camera.main;
			}

			m_cachedBrains = FindObjectsByType<NPCBrain>(FindObjectsSortMode.None);
			for (int i = 0; i < m_cachedBrains.Length; i++)
			{
				NPCBrain brain = m_cachedBrains[i];
				if (brain == null)
				{
					continue;
				}

				NPCDebugLabel label = brain.GetComponentInChildren<NPCDebugLabel>(true);
				if (label == null)
				{
					GameObject go = new GameObject("NPCDebugLabel");
					go.transform.SetParent(brain.transform, false);
					TextMeshPro text = go.AddComponent<TextMeshPro>();
					text.fontSize = 2.6f;
					text.alignment = TextAlignmentOptions.Center;
					label = go.AddComponent<NPCDebugLabel>();
				}

				label.FollowTarget = brain.transform;
				bool visible = IsInRange(brain.transform.position);
				label.gameObject.SetActive(visible);
				label.SetVisible(visible);
			}
		}

		private bool IsInRange(Vector3 position)
		{
			if (m_camera == null)
			{
				return true;
			}

			float sqr = (position - m_camera.transform.position).sqrMagnitude;
			return sqr <= cullDistance * cullDistance;
		}
	}
}

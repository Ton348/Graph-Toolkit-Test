using Prototype.Business.NPC.AI;
using Prototype.Business.NPC.Movement;
using Prototype.Business.NPC.Needs;
using TMPro;
using UnityEngine;

namespace Prototype.Business.NPC.Debug
{
	public sealed class NPCDebugLabel : MonoBehaviour
	{
		[SerializeField] private Vector3 worldOffset = new(0f, 2.2f, 0f);
		[SerializeField] private float updateInterval = 0.25f;

		private NPCBrain m_brain;
		private NPCNeedsComponent m_needs;
		private NPCMovementController m_movement;
		private TextMeshPro m_text;
		private float m_timer;

		public Transform FollowTarget { get; set; }

		private void Awake()
		{
			m_brain = GetComponentInParent<NPCBrain>();
			m_needs = GetComponentInParent<NPCNeedsComponent>();
			m_movement = GetComponentInParent<NPCMovementController>();
			m_text = GetComponent<TextMeshPro>();
			if (m_text == null)
			{
				m_text = gameObject.AddComponent<TextMeshPro>();
			}

			m_text.alignment = TextAlignmentOptions.Center;
			m_text.fontSize = 2.6f;
			m_text.color = Color.white;
			m_text.enableWordWrapping = false;
		}

		private void LateUpdate()
		{
			if (FollowTarget != null)
			{
				transform.position = FollowTarget.position + worldOffset;
			}

			Camera cam = Camera.main;
			if (cam != null)
			{
				Vector3 dir = transform.position - cam.transform.position;
				if (dir.sqrMagnitude > 0.0001f)
				{
					transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
				}
			}

			m_timer += UnityEngine.Time.deltaTime;
			if (m_timer < Mathf.Max(0.05f, updateInterval))
			{
				return;
			}

			m_timer = 0f;
			UpdateText();
		}

		public void SetVisible(bool visible)
		{
			if (m_text != null)
			{
				m_text.enabled = visible;
			}
		}

		private void UpdateText()
		{
			if (m_text == null)
			{
				return;
			}

			string goal = m_brain != null ? m_brain.CurrentActionLabel : "WANDERING";
			string movement = m_movement != null ? m_movement.CurrentState.ToString().ToUpperInvariant() : "IDLE";
			string target = m_brain != null && !string.IsNullOrWhiteSpace(m_brain.CurrentTargetBusinessName)
				? m_brain.CurrentTargetBusinessName
				: movement;

			float hunger = m_needs != null ? m_needs.Hunger : 0f;
			float toilet = m_needs != null ? m_needs.Toilet : 0f;
			float energy = m_needs != null ? m_needs.Energy : 0f;

			m_text.text =
				$"{goal}\nH:{Mathf.RoundToInt(hunger)} T:{Mathf.RoundToInt(toilet)} E:{Mathf.RoundToInt(energy)}\nTarget: {target}";
		}
	}
}

using TMPro;
using UnityEngine;

namespace Prototype.Business.NPC.Workers
{
	[RequireComponent(typeof(TextMeshPro))]
	public sealed class WorkerNPCDebugLabel : MonoBehaviour
	{
		[SerializeField] private Vector3 offset = new(0f, 2f, 0f);
		[SerializeField] private float updateSeconds = 0.25f;

		private WorkerNPCBrain m_brain;
		private TextMeshPro m_text;
		private float m_timer;

		private void Awake()
		{
			m_brain = GetComponentInParent<WorkerNPCBrain>();
			m_text = GetComponent<TextMeshPro>();
			m_text.alignment = TextAlignmentOptions.Center;
			m_text.fontSize = 2.3f;
		}

		private void LateUpdate()
		{
			if (m_brain == null)
			{
				return;
			}

			transform.position = m_brain.transform.position + offset;
			Camera cam = Camera.main;
			if (cam != null)
			{
				Vector3 dir = transform.position - cam.transform.position;
				if (dir.sqrMagnitude > 0.001f)
				{
					transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
				}
			}

			m_timer += Time.deltaTime;
			if (m_timer < Mathf.Max(0.05f, updateSeconds))
			{
				return;
			}
			m_timer = 0f;
			m_text.text = m_brain.DebugStateText;
		}
	}
}

using UnityEngine;

namespace Dreamteck.Splines
{
	//Use the CreateAssetMenu attribute to add the object to the Create Asset context menu
	//After that, go to Assets/Create/Dreamteck/Splines/... and create the scriptable object
	[CreateAssetMenu(menuName = "Dreamteck/Splines/Object Controller Rules/Spiral Rule")]
	public class ObjectControllerSpiralRule : ObjectControllerCustomRuleBase
	{
		[SerializeField]
		private bool m_useSplinePercent;

		[SerializeField]
		private float m_revolve = 360f;

		[SerializeField]
		private Vector2 m_startSize = Vector2.one;

		[SerializeField]
		private Vector2 m_endSize = Vector2.one;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_offset;

		public bool useSplinePercent
		{
			get => m_useSplinePercent;
			set => m_useSplinePercent = value;
		}

		public float revolve
		{
			get => m_revolve;
			set => m_revolve = value;
		}

		public Vector2 startSize
		{
			get => m_startSize;
			set => m_startSize = value;
		}

		public Vector2 endSize
		{
			get => m_endSize;
			set => m_endSize = value;
		}

		public float offset
		{
			get => m_offset;
			set
			{
				m_offset = value;
				if (m_offset > 1)
				{
					m_offset -= Mathf.FloorToInt(m_offset);
				}

				if (m_offset < 0)
				{
					m_offset += Mathf.FloorToInt(-m_offset);
				}
			}
		}

		//Override GetOffset, GetRotation and GetScale to implement custom behaviors
		//Use the information from currentSample, currentObjectIndex, totalObjects and currentObjectPercent

		public override Vector3 GetOffset()
		{
			Vector3 offset = Quaternion.AngleAxis(m_revolve * GetPercent(), Vector3.forward) * Vector3.up;
			Vector2 scale = Vector2.Lerp(m_startSize, m_endSize, GetPercent());
			offset.x *= scale.x;
			offset.y *= scale.y;
			return offset;
		}

		public override Quaternion GetRotation()
		{
			return m_currentSample.rotation * Quaternion.AngleAxis(m_revolve * -GetPercent(), Vector3.forward);
		}

		private float GetPercent()
		{
			float percent = m_useSplinePercent ? (float)m_currentSample.percent : currentObjectPercent + m_offset;
			if (percent > 1)
			{
				percent -= Mathf.FloorToInt(percent);
			}

			if (percent < 0)
			{
				percent += Mathf.FloorToInt(-percent);
			}

			return percent;
		}
	}
}
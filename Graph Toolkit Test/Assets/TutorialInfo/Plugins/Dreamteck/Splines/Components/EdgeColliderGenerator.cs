using UnityEngine;

namespace Dreamteck.Splines
{
	[AddComponentMenu("Dreamteck/Splines/Users/Edge Collider Generator")]
	[RequireComponent(typeof(EdgeCollider2D))]
	public class EdgeColliderGenerator : SplineUser
	{
		[SerializeField]
		[HideInInspector]
		private float m_offset;

		[SerializeField]
		[HideInInspector]
		protected EdgeCollider2D m_edgeCollider;

		[SerializeField]
		[HideInInspector]
		protected Vector2[] m_vertices = new Vector2[0];

		[HideInInspector]
		public float updateRate = 0.1f;

		protected float m_lastUpdateTime;

		private bool m_updateCollider;

		public float offset
		{
			get => m_offset;
			set
			{
				if (value != m_offset)
				{
					m_offset = value;
					Rebuild();
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			m_edgeCollider = GetComponent<EdgeCollider2D>();
		}


		protected override void Reset()
		{
			base.Reset();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		protected override void LateRun()
		{
			base.LateRun();
			if (m_updateCollider)
			{
				if (m_edgeCollider != null)
				{
					if (Time.time - m_lastUpdateTime >= updateRate)
					{
						m_lastUpdateTime = Time.time;
						m_updateCollider = false;
						m_edgeCollider.points = m_vertices;
					}
				}
			}
		}

		protected override void Build()
		{
			base.Build();
			if (m_vertices.Length != sampleCount)
			{
				m_vertices = new Vector2[sampleCount];
			}

			bool hasOffset = offset != 0f;
			for (var i = 0; i < sampleCount; i++)
			{
				GetSample(i, ref m_evalResult);
				m_vertices[i] = m_evalResult.position;
				if (hasOffset)
				{
					Vector2 right = new Vector2(-m_evalResult.forward.y, m_evalResult.forward.x).normalized *
					                m_evalResult.size;
					m_vertices[i] += right * offset;
				}
			}
		}

		protected override void PostBuild()
		{
			base.PostBuild();
			if (m_edgeCollider == null)
			{
				return;
			}

			for (var i = 0; i < m_vertices.Length; i++)
			{
				m_vertices[i] = transform.InverseTransformPoint(m_vertices[i]);
			}

#if UNITY_EDITOR
			if (!Application.isPlaying || updateRate <= 0f)
			{
				m_edgeCollider.points = m_vertices;
			}
			else
			{
				m_updateCollider = true;
			}
#else
            if(updateRate == 0f) edgeCollider.points = vertices;
            else updateCollider = true;
#endif
		}
	}
}
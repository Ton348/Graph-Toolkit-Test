using UnityEngine;
using System.Collections;
using System.Threading;
namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Users/Polygon Collider Generator")]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonColliderGenerator : SplineUser
    {
        public enum Type { Path, Shape }
        public Type type
        {
            get
            {
                return m_type;
            }
            set
            {
                if (value != m_type)
                {
                    m_type = value;
                    Rebuild();
                }
            }
        }

        public float size
        {
            get { return m_size; }
            set
            {
                if (value != m_size)
                {
                    m_size = value;
                    Rebuild();
                }
            }
        }

        public float offset
        {
            get { return m_offset; }
            set
            {
                if (value != m_offset)
                {
                    m_offset = value;
                    Rebuild();
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private Type m_type = Type.Path;
        [SerializeField]
        [HideInInspector]
        private float m_size = 1f;
        [SerializeField]
        [HideInInspector]
        private float m_offset = 0f;
        [SerializeField]
        [HideInInspector]
        protected PolygonCollider2D m_polygonCollider;

        [SerializeField]
        [HideInInspector]
        protected Vector2[] m_vertices = new Vector2[0];

        [HideInInspector]
        public float updateRate = 0.1f;
        protected float m_lastUpdateTime = 0f;

        private bool m_updateCollider = false;

        protected override void Awake()
        {
            base.Awake();
            m_polygonCollider = GetComponent<PolygonCollider2D>();
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
                if (m_polygonCollider != null)
                {
                    if (Time.time - m_lastUpdateTime >= updateRate)
                    {
                        m_lastUpdateTime = Time.time;
                        m_updateCollider = false;
                        m_polygonCollider.SetPath(0, m_vertices);
                    }
                }
            }
        }

        protected override void Build()
        {
            base.Build();
            switch(type){
                case Type.Path:
                GeneratePath();
                break;
                case Type.Shape: GenerateShape(); break;
            }

        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (m_polygonCollider == null) return;
            for(int i = 0; i < m_vertices.Length; i++)
            {
                m_vertices[i] = transform.InverseTransformPoint(m_vertices[i]);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying || updateRate <= 0f) m_polygonCollider.SetPath(0, m_vertices);
            else m_updateCollider = true;
#else
            if(updateRate == 0f) polygonCollider.SetPath(0, vertices);
            else updateCollider = true;
#endif
        }

        private void GeneratePath()
        {
            int vertexCount = sampleCount * 2;
            if (m_vertices.Length != vertexCount) m_vertices = new Vector2[vertexCount];
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, ref m_evalResult);
                Vector2 right = new Vector2(-m_evalResult.forward.y, m_evalResult.forward.x).normalized * m_evalResult.size;
                m_vertices[i] = new Vector2(m_evalResult.position.x, m_evalResult.position.y) + right * size * 0.5f + right * offset;
                m_vertices[sampleCount + (sampleCount - 1) - i] = new Vector2(m_evalResult.position.x, m_evalResult.position.y) - right * size * 0.5f + right * offset;
            }
        }

        private void GenerateShape()
        {
            if (m_vertices.Length != sampleCount) m_vertices = new Vector2[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, ref m_evalResult);
                m_vertices[i] = m_evalResult.position;
                if (offset != 0f)
                {
                    Vector2 right = new Vector2(-m_evalResult.forward.y, m_evalResult.forward.x).normalized * m_evalResult.size;
                    m_vertices[i] += right * offset;
                }
            }
        }
    }

  
}

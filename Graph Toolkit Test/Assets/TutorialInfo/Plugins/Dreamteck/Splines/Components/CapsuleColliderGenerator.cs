namespace Dreamteck.Splines
{
    using UnityEngine;

    public class CapsuleColliderGenerator : SplineUser, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector, Min(0f)] private float m_radius = 1f;
        [SerializeField, HideInInspector, Min(0f)] private float m_height = 1f;
        [SerializeField, HideInInspector] private bool m_overlapCaps = true;
        [SerializeField, HideInInspector] private CapsuleColliderZdirection m_direction = CapsuleColliderZdirection.Z;
        [SerializeField, HideInInspector] private ColliderObject[] m_colliders = new ColliderObject[0];

        public float radius
        {
            get { return m_radius; }
            set
            {
                if (value != m_radius)
                {
                    m_radius = value;
                    Rebuild();
                }
            }
        }

        public float height
        {
            get { return m_height; }
            set
            {
                if (value != m_height)
                {
                    m_height = value;
                    Rebuild();
                }
            }
        }

        public bool overlapCaps
        {
            get { return m_overlapCaps; }
            set
            {
                if (value != m_overlapCaps)
                {
                    m_overlapCaps = value;
                    Rebuild();
                }
            }
        }

        public CapsuleColliderZdirection direction
        {
            get { return m_direction; }
            set
            {
                if (value != m_direction)
                {
                    m_direction = value;
                    Rebuild();
                }
            }
        }

        private void DestroyCollider(ColliderObject collider)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(collider.transform.gameObject);
            }
            else
            {
                DestroyImmediate(collider.transform.gameObject);
            }
#else
            Destroy(collider.transform.gameObject);
#endif
        }

        protected override void Build()
        {
            base.Build();

            if (sampleCount == 0)
            {
                for (int i = 0; i < m_colliders.Length; i++)
                {
                    DestroyCollider(m_colliders[i]);
                }
                m_colliders = new ColliderObject[0];
                return;
            }

            int objectCount = sampleCount - 1;
            if (objectCount != m_colliders.Length)
            {
                GenerateColliders(objectCount);
            }

            SplineSample current = new SplineSample();
            SplineSample next = new SplineSample();
            Evaluate(0.0, ref current);

            bool controlHeight = m_direction == CapsuleColliderZdirection.Z;

            for (int i = 0; i < objectCount; i++)
            {
                double nextPercent = (double)(i + 1) / (sampleCount - 1);
                Evaluate(nextPercent, ref next);
                m_colliders[i].transform.position = Vector3.Lerp(current.position, next.position, 0.5f);
                m_colliders[i].transform.rotation = Quaternion.LookRotation(next.position - current.position, Vector3.Slerp(current.up, next.up, 0.5f));
                
                m_colliders[i].collider.radius = m_radius;
                m_colliders[i].collider.direction = (int)m_direction;

                var distance = Vector3.Distance(current.position, next.position);

                if (controlHeight)
                {
                    if (m_overlapCaps)
                    {
                        m_colliders[i].collider.height = distance + m_radius * 2f;
                    } else
                    {
                        m_colliders[i].collider.height = distance;
                    }
                    m_colliders[i].collider.radius = m_radius;
                }
                else
                {
                    m_colliders[i].collider.height = m_height;
                    m_colliders[i].collider.radius = distance * 0.5f;
                }

                current = next;
            }
        }

        private void GenerateColliders(int count)
        {
            ColliderObject[] newColliders = new ColliderObject[count];
            for (int i = 0; i < newColliders.Length; i++)
            {
                if (i < m_colliders.Length)
                {
                    newColliders[i] = m_colliders[i];
                }
                else
                {
                    GameObject newObject = new GameObject("Collider " + i);
                    newObject.layer = gameObject.layer;
                    newObject.transform.parent = trs;
                    newColliders[i] = new ColliderObject(newObject.transform, newObject.AddComponent<CapsuleCollider>(), m_direction, m_height);
                }
            }
            if (newColliders.Length < m_colliders.Length)
            {
                for (int i = newColliders.Length; i < m_colliders.Length; i++)
                {
                    DestroyCollider(m_colliders[i]);
                }
            }
            m_colliders = newColliders;
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            Build();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < m_colliders.Length; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(m_colliders[i].transform.gameObject);
                }
                else
                {
                    Destroy(m_colliders[i].transform.gameObject);
                }
#else
                Destroy(_colliders[i].transform.gameObject);
#endif
            }
        }

        [System.Serializable]
        public class ColliderObject
        {
            public Transform transform;
            public CapsuleCollider collider;

            public ColliderObject(Transform transform, CapsuleCollider collider, CapsuleColliderZdirection direction, float height)
            {
                this.transform = transform;
                this.collider = collider;
                this.collider.direction = (int)direction;
                this.collider.height = height;
            }
        }

        public enum CapsuleColliderZdirection
        {
            X = 0, Y = 1, Z = 2,
        }
    }
}
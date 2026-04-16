using UnityEngine;
using System.Collections;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace Dreamteck.Splines
{
    public class MeshGenerator : SplineUser
    {
        protected const int s_uNity16VertexLimit = 65535;

        public float size
        {
            get { return m_size; }
            set
            {
                if (value != m_size)
                {
                    m_size = value;
                    Rebuild();
                } else m_size = value;
            }
        }

        public Color color
        {
            get { return m_color; }
            set
            {
                if (value != m_color)
                {
                    m_color = value;
                    Rebuild();
                }
            }
        }

        public Vector3 offset
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

        public NormalMethod normalMethod
        {
            get { return m_normalMethod; }
            set
            {
                if (value != m_normalMethod)
                {
                    m_normalMethod = value;
                    Rebuild();
                }
            }
        }

        public bool useSplineSize
        {
            get { return m_useSplineSize; }
            set
            {
                if (value != m_useSplineSize)
                {
                    m_useSplineSize = value;
                    Rebuild();
                }
            }
        }

        public bool useSplineColor
        {
            get { return m_useSplineColor; }
            set
            {
                if (value != m_useSplineColor)
                {
                    m_useSplineColor = value;
                    Rebuild();
                }
            }
        }

        public bool calculateTangents
        {
            get { return m_calculateTangents; }
            set
            {
                if (value != m_calculateTangents)
                {
                    m_calculateTangents = value;
                    Rebuild();
                }
            }
        }

        public float rotation
        {
            get { return m_rotation; }
            set
            {
                if (value != m_rotation)
                {
                    m_rotation = value;
                    Rebuild();
                }
            }
        }

        public bool flipFaces
        {
            get { return m_flipFaces; }
            set
            {
                if (value != m_flipFaces)
                {
                    m_flipFaces = value;
                    Rebuild();
                }
            }
        }

        public bool doubleSided
        {
            get { return m_doubleSided; }
            set
            {
                if (value != m_doubleSided)
                {
                    m_doubleSided = value;
                    Rebuild();
                }
            }
        }

        public Uvmode uvMode
        {
            get { return m_uvMode; }
            set
            {
                if (value != m_uvMode)
                {
                    m_uvMode = value;
                    Rebuild();
                }
            }
        }

        public Vector2 uvScale
        {
            get { return m_uvScale; }
            set
            {
                if (value != m_uvScale)
                {
                    m_uvScale = value;
                    Rebuild();
                }
            }
        }

        public Vector2 uvOffset
        {
            get { return m_uvOffset; }
            set
            {
                if (value != m_uvOffset)
                {
                    m_uvOffset = value;
                    Rebuild();
                }
            }
        }

        public float uvRotation
        {
            get { return m_uvRotation; }
            set
            {
                if (value != m_uvRotation)
                {
                    m_uvRotation = value;
                    Rebuild();
                }
            }
        }

        public UnityEngine.Rendering.IndexFormat meshIndexFormat
        {
            get { return m_meshIndexFormat; }
            set
            {
                if (value != m_meshIndexFormat)
                {
                    m_meshIndexFormat = value;
                    RefreshMesh();
                    Rebuild();
                }
            }
        }

        public bool baked
        {
            get
            {
                return m_baked;
            }
        }

        public bool markDynamic
        {
            get { return m_markDynamic; }
            set
            {
                if (value != m_markDynamic)
                {
                    m_markDynamic = value;
                    RefreshMesh();
                    Rebuild();
                }
            }
        }

        public enum Uvmode { Clip, UniformClip, Clamp, UniformClamp }
        public enum NormalMethod { Recalculate, SplineNormals }
        [SerializeField]
        [HideInInspector]
        private bool m_baked = false;
        [SerializeField]
        [HideInInspector]
        private bool m_markDynamic = true;
        [SerializeField]
        [HideInInspector]
        private float m_size = 1f;
        [SerializeField]
        [HideInInspector]
        private Color m_color = Color.white;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_offset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private NormalMethod m_normalMethod = NormalMethod.SplineNormals;
        [SerializeField]
        [HideInInspector]
        private bool m_calculateTangents = true;
        [SerializeField]
        [HideInInspector]
        private bool m_useSplineSize = true;
        [SerializeField]
        [HideInInspector]
        private bool m_useSplineColor = true;
        [SerializeField]
        [HideInInspector]
        [Range(-360f, 360f)]
        private float m_rotation = 0f;
        [SerializeField]
        [HideInInspector]
        private bool m_flipFaces = false;
        [SerializeField]
        [HideInInspector]
        private bool m_doubleSided = false;
        [SerializeField]
        [HideInInspector]
        private Uvmode m_uvMode = Uvmode.Clip;
        [SerializeField]
        [HideInInspector]
        private Vector2 m_uvScale = Vector2.one;
        [SerializeField]
        [HideInInspector]
        private Vector2 m_uvOffset = Vector2.zero;
        [SerializeField]
        [HideInInspector]
        private float m_uvRotation = 0f;
        [SerializeField]
        [HideInInspector]
        private UnityEngine.Rendering.IndexFormat m_meshIndexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        [SerializeField]
        [HideInInspector]
        private Mesh m_bakedMesh;

        [HideInInspector]
        public float colliderUpdateRate = 0.2f;
        protected bool m_updateCollider = false;
        protected float m_lastUpdateTime = 0f;

        protected float m_vDist = 0f;
        protected static Vector2 s_uvs = Vector2.zero;

        protected virtual string meshName => "Mesh";
        protected TsMesh tsMesh { get; private set; }
        protected Mesh m_mesh;

        protected MeshFilter m_filter;
        protected MeshRenderer m_meshRenderer;
        protected MeshCollider m_meshCollider;

#if UNITY_EDITOR

        public void Bake(bool makeStatic, bool lightmapUv)
        {
            if (m_mesh == null) return;
            gameObject.isStatic = false;
            UnityEditor.MeshUtility.Optimize(m_mesh);
            if (spline != null)
            {
                spline.Unsubscribe(this);
            }
            m_filter = GetComponent<MeshFilter>();
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_filter.hideFlags = m_meshRenderer.hideFlags = HideFlags.None;
            m_bakedMesh = Instantiate(m_mesh);
            m_bakedMesh.name = meshName + " - Baked";
            if (lightmapUv)
            {
                Unwrapping.GenerateSecondaryUVSet(m_bakedMesh);
            }
            m_filter.sharedMesh = m_bakedMesh;
            m_mesh = null;
            gameObject.isStatic = makeStatic; 
            m_baked = true;
        }

        public void Unbake()
        {
            gameObject.isStatic = false; 
            m_baked = false;
            DestroyImmediate(m_bakedMesh);
            m_bakedMesh = null;
            CreateMesh();
            spline.Subscribe(this);
            Rebuild();
        }

        public override void EditorAwake()
        {
            GetComponents();
            base.EditorAwake();
        }
#endif


        protected override void Awake()
        {
            GetComponents();
            base.Awake();
        }

        protected override void Reset()
        {
            base.Reset();
            GetComponents();
#if UNITY_EDITOR
            bool materialFound = false;
            for (int i = 0; i < m_meshRenderer.sharedMaterials.Length; i++)
            {
                if (m_meshRenderer.sharedMaterials[i] != null)
                {
                    materialFound = true;
                    break;
                }
            }
            if (!materialFound) m_meshRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
#endif
        }

        private void GetComponents()
        {
            m_filter = GetComponent<MeshFilter>();
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_meshCollider = GetComponent<MeshCollider>();
        }

        public override void Rebuild()
        {
            if (m_baked) return;
            base.Rebuild();
        }

        public override void RebuildImmediate()
        {
            if (m_baked) return;
            base.RebuildImmediate();
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
            MeshFilter filter = GetComponent<MeshFilter>();
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if (filter != null)  filter.hideFlags = HideFlags.None;
            if (rend != null)  rend.hideFlags = HideFlags.None;
        }


        public void UpdateCollider()
        {
            m_meshCollider = GetComponent<MeshCollider>();
            if (m_meshCollider == null) m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_meshCollider.sharedMesh = m_filter.sharedMesh;
        }

        protected override void LateRun()
        {
            if (m_baked) return;
            base.LateRun();
            if (m_updateCollider)
            {
                if (m_meshCollider != null)
                {
                    if (Time.time - m_lastUpdateTime >= colliderUpdateRate)
                    {
                        m_lastUpdateTime = Time.time;
                        m_updateCollider = false;
                        m_meshCollider.sharedMesh = m_filter.sharedMesh;
                    }
                }
            }
        }

        protected override void Build()
        {
            base.Build();
            if (tsMesh == null || m_mesh == null)
            {
                CreateMesh();
            }

            if (sampleCount > 1)
            {
                BuildMesh();
            } else
            {
                ClearMesh();
            }
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            WriteMesh();
        }

        protected virtual void ClearMesh()
        {
            tsMesh.Clear();
            m_mesh.Clear();
        }

        protected virtual void BuildMesh()
        {
            //Logic for mesh generation, automatically called in the Build method
        }

        protected virtual void WriteMesh() 
        {
            MeshUtility.TransformMesh(tsMesh, trs.worldToLocalMatrix);
            if (m_doubleSided)
            {
                MeshUtility.MakeDoublesidedHalf(tsMesh);
            }
            else if (m_flipFaces)
            {
                MeshUtility.FlipFaces(tsMesh);
            }

            if (m_calculateTangents)
            {
                MeshUtility.CalculateTangents(tsMesh);
            }

            if (m_meshIndexFormat == UnityEngine.Rendering.IndexFormat.UInt16 && tsMesh.vertexCount > s_uNity16VertexLimit)
            {
                Debug.LogError("WARNING: The generated mesh for " + name + " exceeds the maximum vertex count for standard meshes in Unity (" + s_uNity16VertexLimit + "). To create bigger meshes, set the Index Format inside the Vertices foldout to 32.");
            }

            tsMesh.indexFormat = m_meshIndexFormat;

            tsMesh.WriteMesh(ref m_mesh);

            if (m_markDynamic)
            {
                m_mesh.MarkDynamic();
            }

            if (m_normalMethod == 0)
            {
                m_mesh.RecalculateNormals();
            }

            if (m_filter != null)
            {
                m_filter.sharedMesh = m_mesh;
            }
            m_updateCollider = true;
        }

        protected virtual void AllocateMesh(int vertexCount, int trisCount)
        {
            if(trisCount < 0)
            {
                trisCount = 0;
            }
            if(vertexCount < 0)
            {
                vertexCount = 0;
            }
            if (m_doubleSided)
            {
                vertexCount *= 2;
                trisCount *= 2;
            }
            if (tsMesh.vertexCount != vertexCount)
            {
                tsMesh.vertices = new Vector3[vertexCount];
                tsMesh.normals = new Vector3[vertexCount];
                tsMesh.tangents = new Vector4[vertexCount];
                tsMesh.colors = new Color[vertexCount];
                tsMesh.uv = new Vector2[vertexCount];
            }
            if (tsMesh.triangles.Length != trisCount)
            {
                tsMesh.triangles = new int[trisCount];
            }
        }

        protected void ResetUvdistance()
        {
            m_vDist = 0f;
            if (uvMode == Uvmode.UniformClip)
            {
                m_vDist = spline.CalculateLength(0.0, GetSamplePercent(0));
            }
        }

        protected void AddUvdistance(int sampleIndex)
        {
            if (sampleIndex == 0) return;
            SplineSample current = new SplineSample();
            SplineSample last = new SplineSample();
            GetSampleRaw(sampleIndex, ref current);
            GetSampleRaw(sampleIndex - 1, ref last);
            m_vDist += Vector3.Distance(current.position, last.position);
        }

        protected void CalculateUvs(double percent, float u)
        {
            s_uvs.x = u * m_uvScale.x - m_uvOffset.x;
            switch (uvMode)
            {
                case Uvmode.Clip:  s_uvs.y = CalculateUvclip(percent); break;
                case Uvmode.Clamp: s_uvs.y = CalculateUvclamp(percent);  break;
                case Uvmode.UniformClamp: s_uvs.y = CalculateUvuniformClamp(m_vDist); break;
                default: s_uvs.y = CalculateUvuniformClip(m_vDist); break;
            }
        }

        protected float CalculateUvuniformClamp(float distance)
        {
            return distance * m_uvScale.y / (float)span - m_uvOffset.y;
        }

        protected float CalculateUvuniformClip(float distance)
        {
            return distance * m_uvScale.y - m_uvOffset.y;
        }

        protected float CalculateUvclip(double percent)
        {
            return (float)percent * m_uvScale.y - m_uvOffset.y;
        }

        protected float CalculateUvclamp(double percent)
        {
            return (float)Dmath.InverseLerp(clipFrom, clipTo, percent) * m_uvScale.y - m_uvOffset.y;
        }

        protected float GetBaseSize(SplineSample sample)
        {
            return m_useSplineSize? sample.size: 1f;
        }

        protected Color GetBaseColor(SplineSample sample)
        {
            return m_useSplineColor ? sample.color : Color.white;
        }

        protected virtual void CreateMesh()
        {
            tsMesh = new TsMesh();
            m_mesh = new Mesh();
            m_mesh.name = meshName;
            m_mesh.indexFormat = m_meshIndexFormat;
            tsMesh.indexFormat = m_meshIndexFormat;
            if (m_markDynamic)
            {
                m_mesh.MarkDynamic();
            }
        }

        private void RefreshMesh()
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(m_mesh);
            } 
            else
            {
                Destroy(m_mesh);
            }
            m_mesh = null;
            tsMesh.Clear();
            tsMesh = null;
            CreateMesh();
        }
    }

  
}

using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Surface Generator")]
    public class SurfaceGenerator : MeshGenerator
    {
        public float expand
        {
            get { return m_expand; }
            set
            {
                if (value != m_expand)
                {
                    m_expand = value;
                    Rebuild();
                }
            }
        }

        public float extrude
        {
            get { return m_extrude; }
            set
            {
                if (value != m_extrude)
                {
                    m_extrude = value;
                    Rebuild();
                }
            }
        }

        public double extrudeClipFrom
        {
            get { return m_extrudeFrom; }
            set
            {
                if (value != m_extrudeFrom)
                {
                    m_extrudeFrom = value;
                    Rebuild();
                }
            }
        }

        public double extrudeClipTo
        {
            get { return m_extrudeTo; }
            set
            {
                if (value != m_extrudeTo)
                {
                    m_extrudeTo = value;
                    Rebuild();
                }
            }
        }

        public Vector2 sideUvScale
        {
            get { return m_sideUvScale; }
            set
            {
                if (value != m_sideUvScale)
                {
                    m_sideUvScale = value;
                    Rebuild();
                }
                else
                {
                    m_sideUvScale = value;
                }
            }
        }

        public Vector2 sideUvOffset
        {
            get { return m_sideUvOffset; }
            set
            {
                if (value != m_sideUvOffset)
                {
                    m_sideUvOffset = value;
                    Rebuild();
                }
                else
                {
                    m_sideUvOffset = value;
                }
            }
        }

        public float sideUvRotation
        {
            get { return m_sideUvRotation; }
            set
            {
                if (value != m_sideUvRotation)
                {
                    m_sideUvRotation = value;
                    Rebuild();
                }
                else
                {
                    m_sideUvRotation = value;
                }
            }
        }

        public SplineComputer extrudeSpline
        {
            get { return m_extrudeSpline; }
            set
            {
                if (value != m_extrudeSpline)
                {
                    if (m_extrudeSpline != null)
                    {
                        m_extrudeSpline.Unsubscribe(this);
                    }
                    m_extrudeSpline = value;
                    if (value != null)
                    {
                        m_extrudeSpline.Subscribe(this);
                    }
                    Rebuild();
                }
            }
        }

        public Vector3 extrudeOffset
        {
            get { return m_extrudeOffset; }
            set { 
                if(value != m_extrudeOffset)
                {
                    m_extrudeOffset = value;
                    Rebuild();
                } 
            }
        }

        public bool uniformUvs
        {
            get { return m_uniformUvs; }
            set
            {
                if (value != m_uniformUvs)
                {
                    m_uniformUvs = value;
                    Rebuild();
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float m_expand = 0f;
        [SerializeField]
        [HideInInspector]
        private float m_extrude = 0f;
        [SerializeField]
        [HideInInspector]
        private Vector2 m_sideUvScale = Vector2.one;
        [SerializeField]
        [HideInInspector]
        private Vector2 m_sideUvOffset = Vector2.zero;
        [SerializeField]
        [HideInInspector]
        private float m_sideUvRotation = 0f;
        [SerializeField]
        [HideInInspector]
        private SplineComputer m_extrudeSpline;
        [SerializeField]
        [HideInInspector]
        private Vector3 m_extrudeOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private SplineSample[] m_extrudeResults = new SplineSample[0];
        [SerializeField]
        [HideInInspector]
        private Vector3[] m_identityVertices = new Vector3[0];
        [SerializeField]
        [HideInInspector]
        private Vector3[] m_identityNormals = new Vector3[0];
        [SerializeField]
        [HideInInspector]
        private Vector2[] m_projectedVerts = new Vector2[0];
        [SerializeField]
        [HideInInspector]
        private int[] m_surfaceTris = new int[0];
        [SerializeField]
        [HideInInspector]
        private int[] m_wallTris = new int[0];

        [SerializeField]
        [HideInInspector]
        private double m_extrudeFrom = 0.0;
        [SerializeField]
        [HideInInspector]
        private double m_extrudeTo = 1.0;
        [SerializeField]
        [HideInInspector]
        private bool m_uniformUvs = false;

        private Vector3 m_trsRight = Vector3.right;
        private Vector3 m_trsUp = Vector3.up;
        private Vector3 m_trsForward = Vector3.forward;

        protected override string meshName => "Surface";

        protected override void Awake()
        {
            base.Awake();
            m_trsRight = trs.right;
            m_trsUp = trs.up;
            m_trsForward = trs.forward;
        }

        protected override void BuildMesh()
        {
            if (spline.pointCount == 0) return;
            base.BuildMesh();
            Generate();
        }

        private void LateUpdate()
        {
            if (multithreaded && trs.hasChanged)
            {
                m_trsRight = trs.right;
                m_trsUp = trs.up;
                m_trsForward = trs.forward;
            }
        }

        public void Generate()
        {
            if (!multithreaded)
            {
                m_trsRight = trs.right;
                m_trsUp = trs.up;
                m_trsForward = trs.forward;
            }
            int surfaceVertexCount = sampleCount;
            if (spline.isClosed) surfaceVertexCount--;
            int vertexCount = surfaceVertexCount;
            bool pathExtrude = false;
            if (m_extrudeSpline != null)
            {
                m_extrudeSpline.Evaluate(ref m_extrudeResults, m_extrudeFrom, m_extrudeTo);
                pathExtrude = m_extrudeResults.Length > 0;
            } else if(m_extrudeResults.Length > 0)
            {
                m_extrudeResults = new SplineSample[0];
            }

            bool simpleExtrude = !pathExtrude && m_extrude != 0f;

            if (pathExtrude)
            {
                vertexCount *= 2;
                vertexCount += sampleCount * m_extrudeResults.Length;
            }
            else if (simpleExtrude)
            {
                vertexCount *= 4;
                vertexCount += 2;
            }
            
            Vector3 center, normal;
            GetProjectedVertices(surfaceVertexCount, out center, out normal);

            bool clockwise = IsClockwise(m_projectedVerts);
            bool flipCap = false;
            bool flipSide = false;
            if (!clockwise) flipSide = !flipSide;
            if (simpleExtrude && m_extrude < 0f)
            {
                flipCap = !flipCap;
                flipSide = !flipSide;
            }

            GenerateSurfaceTris(flipCap);
            int totalTrisCount = m_surfaceTris.Length;
            if (simpleExtrude)
            {
                totalTrisCount *= 2;
                totalTrisCount += 2 * sampleCount * 2 * 3;
            } else
            {
                totalTrisCount *= 2;
                totalTrisCount += m_extrudeResults .Length * sampleCount * 2 * 3;
            }
            AllocateMesh(vertexCount, totalTrisCount);
            Vector3 off = m_trsRight * offset.x + m_trsUp * offset.y + m_trsForward * offset.z;
            for (int i = 0; i < surfaceVertexCount; i++)
            {
                GetSample(i, ref m_evalResult);
                tsMesh.vertices[i] = m_evalResult.position + off;
                tsMesh.normals[i] = m_evalResult.up;
                tsMesh.colors[i] = m_evalResult.color * color;
            }

            #region UVs
            Vector2 min = m_projectedVerts[0];
            Vector2 max = m_projectedVerts[0];
            for (int i = 1; i < m_projectedVerts.Length; i++)
            {
                if (min.x < m_projectedVerts[i].x) min.x = m_projectedVerts[i].x;
                if (min.y < m_projectedVerts[i].y) min.y = m_projectedVerts[i].y;
                if (max.x > m_projectedVerts[i].x) max.x = m_projectedVerts[i].x;
                if (max.y > m_projectedVerts[i].y) max.y = m_projectedVerts[i].y;
            }

            for (int i = 0; i < m_projectedVerts.Length; i++)
            {
                tsMesh.uv[i].x = Mathf.InverseLerp(max.x, min.x, m_projectedVerts[i].x) * uvScale.x - uvScale.x * 0.5f + uvOffset.x + 0.5f;
                tsMesh.uv[i].y = Mathf.InverseLerp(min.y, max.y, m_projectedVerts[i].y) * uvScale.y - uvScale.y * 0.5f + uvOffset.y + 0.5f;
                tsMesh.uv[i] = Quaternion.AngleAxis(uvRotation, Vector3.forward) * tsMesh.uv[i];
            }
            #endregion


            if (flipCap)
            {
                for (int i = 0; i < surfaceVertexCount; i++)
                {
                    tsMesh.normals[i] *= -1f;
                }
            }

            if (m_expand != 0f)
            {
                for (int i = 0; i < surfaceVertexCount; i++)
                {
                    GetSample(i, ref m_evalResult);
                    tsMesh.vertices[i] += (clockwise ? -m_evalResult.right : m_evalResult.right) * m_expand;
                }
            }

            if (pathExtrude)
            {
                GetIdentityVerts(center, normal, clockwise);
                //Generate cap vertices with flipped normals
                for (int i = 0; i < surfaceVertexCount; i++)
                {
                    Vector3 vertexOffset = TransformOffset(m_extrudeResults[0], m_extrudeOffset);
                    tsMesh.vertices[i + surfaceVertexCount] = m_extrudeResults[0].position + (m_extrudeResults[0].rotation * m_identityVertices[i] + off) + vertexOffset;
                    tsMesh.normals[i + surfaceVertexCount] = -m_extrudeResults[0].forward;
                    tsMesh.colors[i + surfaceVertexCount] = tsMesh.colors[i] * m_extrudeResults[0].color;
                    tsMesh.uv[i + surfaceVertexCount] = new Vector2(1f - tsMesh.uv[i].x, tsMesh.uv[i].y);

                    vertexOffset = TransformOffset(m_extrudeResults[m_extrudeResults.Length - 1], m_extrudeOffset);
                    tsMesh.vertices[i] = m_extrudeResults[m_extrudeResults.Length - 1].position + (m_extrudeResults[m_extrudeResults.Length - 1].rotation * m_identityVertices[i] + off) + vertexOffset;
                    tsMesh.normals[i] = m_extrudeResults[m_extrudeResults.Length - 1].forward;
                    tsMesh.colors[i] *= m_extrudeResults[m_extrudeResults.Length - 1].color;
                }
                //Add wall vertices
                float totalLength = 0f;
                for (int i = 0; i < m_extrudeResults.Length; i++)
                {
                    if (m_uniformUvs && i > 0) totalLength += Vector3.Distance(m_extrudeResults[i].position, m_extrudeResults[i - 1].position);
                    int startIndex = surfaceVertexCount * 2 + i * sampleCount;
                    for (int n = 0; n < m_identityVertices.Length; n++)
                    {
                        Vector3 vertexOffset = TransformOffset(m_extrudeResults[i], m_extrudeOffset);
                        tsMesh.vertices[startIndex + n] = m_extrudeResults[i].position + (m_extrudeResults[i].rotation * m_identityVertices[n] + off) + vertexOffset;
                        tsMesh.normals[startIndex + n] = m_extrudeResults[i].rotation * m_identityNormals[n];
                        if (m_uniformUvs)
                        {
                            tsMesh.uv[startIndex + n] = new Vector2((float)n / (m_identityVertices.Length - 1) * m_sideUvScale.x + m_sideUvOffset.x, totalLength * m_sideUvScale.y + m_sideUvOffset.y);
                        }
                        else
                        {
                            tsMesh.uv[startIndex + n] = new Vector2((float)n / (m_identityVertices.Length - 1) * m_sideUvScale.x + m_sideUvOffset.x, (float)i / (m_extrudeResults.Length - 1) * m_sideUvScale.y + m_sideUvOffset.y);
                        }
                        if (m_sideUvRotation != 0f)
                        {
                            tsMesh.uv[startIndex + n] = Quaternion.AngleAxis(m_sideUvRotation, Vector3.forward) * tsMesh.uv[startIndex + n];
                        }

                        if (clockwise)
                        {
                            tsMesh.uv[startIndex + n].x = 1f - tsMesh.uv[startIndex + n].x;
                        }
                    }
                }
                int written = WriteTris(ref m_surfaceTris, ref tsMesh.triangles, 0, 0, false);
                written = WriteTris(ref m_surfaceTris, ref tsMesh.triangles, surfaceVertexCount, written, true);

                MeshUtility.GeneratePlaneTriangles(ref m_wallTris, sampleCount - 1, m_extrudeResults.Length, flipSide, 0, 0, true);
                WriteTris(ref m_wallTris, ref tsMesh.triangles, surfaceVertexCount * 2, written, false);
            }
            else if (simpleExtrude)
            {
                //Duplicate cap vertices with flipped normals
                for (int i = 0; i < surfaceVertexCount; i++)
                {
                    tsMesh.vertices[i + surfaceVertexCount] = tsMesh.vertices[i];
                    tsMesh.normals[i + surfaceVertexCount] = -tsMesh.normals[i];
                    tsMesh.colors[i + surfaceVertexCount] = tsMesh.colors[i];
                    tsMesh.uv[i + surfaceVertexCount] = new Vector2(1f - tsMesh.uv[i].x, tsMesh.uv[i].y);
                    tsMesh.vertices[i] += normal * m_extrude;
                }

                //Add wall vertices
                for (int i = 0; i < surfaceVertexCount + 1; i++)
                {
                    int index = i;
                    if (i >= surfaceVertexCount) index = i - surfaceVertexCount;
                    GetSample(index, ref m_evalResult);
                    tsMesh.vertices[i + surfaceVertexCount * 2] = tsMesh.vertices[index] - normal * m_extrude;
                    tsMesh.normals[i + surfaceVertexCount * 2] = clockwise ? -m_evalResult.right : m_evalResult.right;
                    tsMesh.colors[i + surfaceVertexCount * 2] = tsMesh.colors[index];
                    tsMesh.uv[i + surfaceVertexCount * 2] = new Vector2((float)i / (surfaceVertexCount - 1) * m_sideUvScale.x + m_sideUvOffset.x, 0f + m_sideUvOffset.y);
                    if (clockwise)
                    {
                        tsMesh.uv[i + surfaceVertexCount * 2].x = 1f - tsMesh.uv[i + surfaceVertexCount * 2].x;
                    }

                    int offsetIndex = i + surfaceVertexCount * 3 + 1;
                    tsMesh.vertices[offsetIndex] = tsMesh.vertices[index];
                    tsMesh.normals[offsetIndex] = tsMesh.normals[i + surfaceVertexCount * 2];
                    tsMesh.colors[offsetIndex] = tsMesh.colors[index];
                    if (m_uniformUvs)
                    {
                        tsMesh.uv[offsetIndex] = new Vector2((float)i / surfaceVertexCount * m_sideUvScale.x + m_sideUvOffset.x, m_extrude * m_sideUvScale.y + m_sideUvOffset.y);
                    }
                    else
                    {
                        tsMesh.uv[offsetIndex] = new Vector2((float)i / surfaceVertexCount * m_sideUvScale.x + m_sideUvOffset.x, 1f * m_sideUvScale.y + m_sideUvOffset.y);
                    }
                    if (m_sideUvRotation != 0f)
                    {
                        tsMesh.uv[offsetIndex] = Quaternion.AngleAxis(m_sideUvRotation, Vector3.forward) * tsMesh.uv[offsetIndex];
                    }
                    if (clockwise)
                    {
                        tsMesh.uv[offsetIndex].x = 1f - tsMesh.uv[offsetIndex].x;
                    }
                }
                int written = WriteTris(ref m_surfaceTris, ref tsMesh.triangles, 0, 0, false);
                written = WriteTris(ref m_surfaceTris, ref tsMesh.triangles, surfaceVertexCount, written, true);

                MeshUtility.GeneratePlaneTriangles(ref m_wallTris, sampleCount - 1, 2, flipSide, 0, 0, true);
                WriteTris(ref m_wallTris, ref tsMesh.triangles, surfaceVertexCount * 2, written, false);
            }
            else
            {
                WriteTris(ref m_surfaceTris, ref tsMesh.triangles, 0, 0, false);
            }
        }

        private void GenerateSurfaceTris(bool flip)
        {
            MeshUtility.Triangulate(m_projectedVerts, ref m_surfaceTris);
            if (flip) MeshUtility.FlipTriangles(ref m_surfaceTris);
        }

        private int WriteTris(ref int[] tris, ref int[] target, int vertexOffset, int trisOffset, bool flip)
        {
            for (int i = trisOffset; i < trisOffset + tris.Length; i += 3)
            {
                if (flip)
                {
                    target[i] = tris[i + 2 - trisOffset] + vertexOffset;
                    target[i + 1] = tris[i + 1 - trisOffset] + vertexOffset;
                    target[i + 2] = tris[i - trisOffset] + vertexOffset;
                }
                else
                {
                    target[i] = tris[i - trisOffset] + vertexOffset;
                    target[i + 1] = tris[i + 1 - trisOffset] + vertexOffset;
                    target[i + 2] = tris[i + 2 - trisOffset] + vertexOffset;
                }
            }
            return trisOffset + tris.Length;
        }

        bool IsClockwise(Vector2[] points2D)
        {
            float sum = 0f;
            for (int i = 1; i < points2D.Length; i++)
            {
                Vector2 v1 = points2D[i];
                Vector2 v2 = points2D[(i + 1) % points2D.Length];
                sum += (v2.x - v1.x) * (v2.y + v1.y);
            }
            sum += (points2D[0].x - points2D[points2D.Length - 1].x) * (points2D[0].y + points2D[points2D.Length - 1].y);
            return sum <= 0f;
        }

        void GetIdentityVerts(Vector3 center, Vector3 normal, bool clockwise)
        {
            Quaternion vertsRotation = Quaternion.Inverse(Quaternion.LookRotation(normal));
            if (m_identityVertices.Length != sampleCount)
            {
                m_identityVertices = new Vector3[sampleCount];
                m_identityNormals = new Vector3[sampleCount];
            }
            for (int i = 0; i < sampleCount; i++)
            {
                GetSampleRaw(i, ref m_evalResult);
                Vector3 right = m_evalResult.right;
                m_identityVertices[i] = vertsRotation * (m_evalResult.position - center + (clockwise ? -right : right) * m_expand);
                m_identityNormals[i] = vertsRotation * (clockwise ? -right : right);
            }
        }

        void GetProjectedVertices(int count, out Vector3 center, out Vector3 normal)
        {
            center = Vector3.zero;
            normal = Vector3.zero;
            Vector3 off = m_trsRight * offset.x + m_trsUp * offset.y + m_trsForward * offset.z;
            for (int i = 0; i < count; i++)
            {
                GetSampleRaw(i, ref m_evalResult);
                center += m_evalResult.position + off;
                normal += m_evalResult.up;
            }
            normal.Normalize();
            center /= count;

            Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);
            Vector3 up = rot * Vector3.up;
            Vector3 right = rot * Vector3.right;
            if (m_projectedVerts.Length != count) m_projectedVerts = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                GetSampleRaw(i, ref m_evalResult);
                Vector3 point = m_evalResult.position + off - center;
                float projectionPointX = Vector3.Project(point, right).magnitude;
                if (Vector3.Dot(point, right) < 0.0f) projectionPointX *= -1f;
                float projectionPointY = Vector3.Project(point, up).magnitude;
                if (Vector3.Dot(point, up) < 0.0f) projectionPointY *= -1f;
                m_projectedVerts[i].x = projectionPointX;
                m_projectedVerts[i].y = projectionPointY;
            }
        }

    }
}

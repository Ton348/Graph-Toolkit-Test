using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Users/Waveform Generator")]
    public class WaveformGenerator : MeshGenerator
    {
        public enum Axis { X, Y, Z }
        public enum Space { World, Local }
        public enum UvwrapMode { Clamp, UniformX, UniformY, Uniform }

        public Axis axis
        {
            get { return m_axis; }
            set
            {
                if (value != m_axis)
                {
                    m_axis = value;
                    Rebuild();
                }
            }
        }

        public bool symmetry
        {
            get { return m_symmetry; }
            set
            {
                if (value != m_symmetry)
                {
                    m_symmetry = value;
                    Rebuild();
                }
            }
        }

        public UvwrapMode uvWrapMode
        {
            get { return m_uvWrapMode; }
            set
            {
                if (value != m_uvWrapMode)
                {
                    m_uvWrapMode = value;
                    Rebuild();
                }
            }
        }

        public int slices
        {
            get { return m_slices; }
            set
            {
                if (value != m_slices)
                {
                    if (value < 1) value = 1;
                    m_slices = value;
                    Rebuild();
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Axis m_axis = Axis.Y;
        [SerializeField]
        [HideInInspector]
        private bool m_symmetry = false;
        [SerializeField]
        [HideInInspector]
        private UvwrapMode m_uvWrapMode = UvwrapMode.Clamp;
        [SerializeField]
        [HideInInspector]
        private int m_slices = 1;

        protected override string meshName => "Waveform";

        protected override void BuildMesh()
        {
            base.BuildMesh();
            Generate();
        }

        protected override void Build()
        {
            base.Build();
        }

        protected override void LateRun()
        {
            base.LateRun();
        }

        private void Generate()
        {
            int vertexCount = sampleCount * (m_slices + 1);
            AllocateMesh(vertexCount, m_slices * (sampleCount - 1) * 6);
            int vertIndex = 0;
            float avgTop = 0f;
            float totalLength = 0f;
            Vector3 computerPosition = spline.position;
            Vector3 normal = spline.TransformDirection(Vector3.right);
            switch (m_axis)
            {
                case Axis.Y: normal = spline.TransformDirection(Vector3.up); break;
                case Axis.Z: normal = spline.TransformDirection(Vector3.forward); break;
            }

            Vector3 lastPosition = Vector3.zero;
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, ref m_evalResult);
                float resultSize = GetBaseSize(m_evalResult);
                Vector3 samplePosition = m_evalResult.position;
                Vector3 localSamplePosition = spline.InverseTransformPoint(samplePosition);
                Vector3 bottomPosition = localSamplePosition;
                Vector3 sampleDirection = m_evalResult.forward;
                Vector3 sampleNormal = m_evalResult.up;

                float heightPercent = 1f;
                if (m_uvWrapMode == UvwrapMode.UniformX || m_uvWrapMode == UvwrapMode.Uniform)
                {
                    if (i > 0)
                    {
                        totalLength += Vector3.Distance(m_evalResult.position, lastPosition);
                    }
                }
                switch (m_axis)
                {
                    case Axis.X: bottomPosition.x = m_symmetry ? -localSamplePosition.x : 0f;  heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.x); avgTop += localSamplePosition.x; break;
                    case Axis.Y: bottomPosition.y = m_symmetry ? -localSamplePosition.y : 0f;  heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.y); avgTop += localSamplePosition.y; break;
                    case Axis.Z: bottomPosition.z = m_symmetry ? -localSamplePosition.z : 0f;  heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.z); avgTop += localSamplePosition.z; break;
                }
                bottomPosition = spline.TransformPoint(bottomPosition);
                Vector3 right = Vector3.Cross(normal, sampleDirection).normalized;
                Vector3 offsetRight = Vector3.Cross(sampleNormal, sampleDirection);
                
                for (int n = 0; n < m_slices + 1; n++)
                {
                    float slicePercent = ((float)n / m_slices);
                    tsMesh.vertices[vertIndex] = Vector3.Lerp(bottomPosition, samplePosition, slicePercent) + normal * (offset.y * resultSize) + offsetRight * (offset.x * resultSize);
                    tsMesh.normals[vertIndex] = right;
                    switch (m_uvWrapMode)
                    {
                        case UvwrapMode.Clamp: tsMesh.uv[vertIndex] = new Vector2((float)m_evalResult.percent * uvScale.x + uvOffset.x, slicePercent * uvScale.y + uvOffset.y); break;
                        case UvwrapMode.UniformX: tsMesh.uv[vertIndex] = new Vector2(totalLength * uvScale.x + uvOffset.x, slicePercent * uvScale.y + uvOffset.y); break;
                        case UvwrapMode.UniformY: tsMesh.uv[vertIndex] = new Vector2((float)m_evalResult.percent * uvScale.x + uvOffset.x, heightPercent * slicePercent * uvScale.y + uvOffset.y); break;
                        case UvwrapMode.Uniform: tsMesh.uv[vertIndex] = new Vector2(totalLength * uvScale.x + uvOffset.x, heightPercent * slicePercent * uvScale.y + uvOffset.y); break;
                    }
                    tsMesh.colors[vertIndex] = GetBaseColor(m_evalResult) * color;
                    vertIndex++;
                }
                lastPosition = m_evalResult.position;
            }
            if (sampleCount > 0) avgTop /= sampleCount;
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, m_slices, sampleCount, avgTop < 0f);
        }
    }
}

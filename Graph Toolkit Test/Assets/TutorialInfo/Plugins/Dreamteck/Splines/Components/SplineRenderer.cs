using UnityEngine;

namespace Dreamteck.Splines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Dreamteck/Splines/Users/Spline Renderer")]
	[ExecuteInEditMode]
	public class SplineRenderer : MeshGenerator
	{
		[HideInInspector]
		public bool autoOrient = true;

		[HideInInspector]
		public int updateFrameInterval;

		[SerializeField]
		[HideInInspector]
		private int m_slices = 1;

		private int m_currentFrame;
		private bool m_init;
		private bool m_orthographic;
		private Vector3 m_vertexDirection = Vector3.up;

		public int slices
		{
			get => m_slices;
			set
			{
				if (value != m_slices)
				{
					if (value < 1)
					{
						value = 1;
					}

					m_slices = value;
					Rebuild();
				}
			}
		}

		private void Start()
		{
			if (Camera.current != null)
			{
				m_orthographic = Camera.current.orthographic;
			}
			else if (Camera.main != null)
			{
				m_orthographic = Camera.main.orthographic;
			}

			CreateMesh();
		}

		private void OnWillRenderObject()
		{
			if (!autoOrient)
			{
				return;
			}

			if (updateFrameInterval > 0)
			{
				if (m_currentFrame != 0)
				{
					return;
				}
			}

			if (!Application.isPlaying)
			{
				if (!m_init)
				{
					Awake();
					m_init = true;
				}
			}

			if (Camera.current != null)
			{
				RenderWithCamera(Camera.current);
			}
			else if (Camera.main)
			{
				RenderWithCamera(Camera.main);
			}
		}

		protected override void LateRun()
		{
			if (updateFrameInterval > 0)
			{
				m_currentFrame++;
				if (m_currentFrame > updateFrameInterval)
				{
					m_currentFrame = 0;
				}
			}
		}

		protected override void BuildMesh()
		{
			base.BuildMesh();
			GenerateVertices(m_vertexDirection, m_orthographic);
			MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, m_slices, sampleCount, false);
		}

		public void RenderWithCamera(Camera cam)
		{
			m_orthographic = cam.orthographic;
			if (m_orthographic)
			{
				m_vertexDirection = -cam.transform.forward;
			}
			else
			{
				m_vertexDirection = cam.transform.position;
			}

			BuildMesh();
			WriteMesh();
		}

		public void GenerateVertices(Vector3 vertexDirection, bool orthoGraphic)
		{
			AllocateMesh((m_slices + 1) * sampleCount, m_slices * (sampleCount - 1) * 6);
			var vertexIndex = 0;
			ResetUvdistance();
			bool hasOffset = offset != Vector3.zero;
			for (var i = 0; i < sampleCount; i++)
			{
				GetSample(i, ref m_evalResult);
				Vector3 center = m_evalResult.position;
				if (hasOffset)
				{
					center += offset.x * -Vector3.Cross(m_evalResult.forward, m_evalResult.up) +
					          offset.y * m_evalResult.up + offset.z * m_evalResult.forward;
				}

				Vector3 vertexNormal;
				if (orthoGraphic)
				{
					vertexNormal = vertexDirection;
				}
				else
				{
					vertexNormal = (vertexDirection - center).normalized;
				}

				Vector3 vertexRight = Vector3.Cross(m_evalResult.forward, vertexNormal).normalized;
				if (uvMode == Uvmode.UniformClamp || uvMode == Uvmode.UniformClip)
				{
					AddUvdistance(i);
				}

				Color vertexColor = m_evalResult.color * color;
				for (var n = 0; n < m_slices + 1; n++)
				{
					float slicePercent = (float)n / m_slices;
					tsMesh.vertices[vertexIndex] = center - vertexRight * m_evalResult.size * 0.5f * size +
					                               vertexRight * m_evalResult.size * slicePercent * size;
					CalculateUvs(m_evalResult.percent, slicePercent);
					tsMesh.uv[vertexIndex] = Vector2.one * 0.5f +
					                         (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                                   (Vector2.one * 0.5f - s_uvs));
					tsMesh.normals[vertexIndex] = vertexNormal;
					tsMesh.colors[vertexIndex] = vertexColor;
					vertexIndex++;
				}
			}
		}
	}
}
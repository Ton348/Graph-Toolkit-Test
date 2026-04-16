using System;
using UnityEngine;

namespace Dreamteck.Splines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Dreamteck/Splines/Users/Path Generator")]
	public class PathGenerator : MeshGenerator
	{
		[SerializeField]
		[HideInInspector]
		private int m_slices = 1;

		[SerializeField]
		[HideInInspector]
		[Tooltip(
			"This will inflate sample sizes based on the angle between two samples in order to preserve geometry width")]
		private bool m_compensateCorners;

		[SerializeField]
		[HideInInspector]
		private bool m_useShapeCurve;

		[SerializeField]
		[HideInInspector]
		private AnimationCurve m_shape;

		[SerializeField]
		[HideInInspector]
		private AnimationCurve m_lastShape;

		[SerializeField]
		[HideInInspector]
		private float m_shapeExposure = 1f;

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

		public bool useShapeCurve
		{
			get => m_useShapeCurve;
			set
			{
				if (value != m_useShapeCurve)
				{
					m_useShapeCurve = value;
					if (m_useShapeCurve)
					{
						m_shape = new AnimationCurve();
						m_shape.AddKey(new Keyframe(0, 0));
						m_shape.AddKey(new Keyframe(1, 0));
					}
					else
					{
						m_shape = null;
					}

					Rebuild();
				}
			}
		}

		public bool compensateCorners
		{
			get => m_compensateCorners;
			set
			{
				if (value != m_compensateCorners)
				{
					m_compensateCorners = value;
					Rebuild();
				}
			}
		}

		public float shapeExposure
		{
			get => m_shapeExposure;
			set
			{
				if (spline != null && value != m_shapeExposure)
				{
					m_shapeExposure = value;
					Rebuild();
				}
			}
		}

		public AnimationCurve shape
		{
			get => m_shape;
			set
			{
				if (m_lastShape == null)
				{
					m_lastShape = new AnimationCurve();
				}

				var keyChange = false;
				if (value.keys.Length != m_lastShape.keys.Length)
				{
					keyChange = true;
				}
				else
				{
					for (var i = 0; i < value.keys.Length; i++)
					{
						if (value.keys[i].inTangent != m_lastShape.keys[i].inTangent ||
						    value.keys[i].outTangent != m_lastShape.keys[i].outTangent ||
						    value.keys[i].time != m_lastShape.keys[i].time ||
						    value.keys[i].value != value.keys[i].value)
						{
							keyChange = true;
							break;
						}
					}
				}

				if (keyChange)
				{
					Rebuild();
				}

				m_lastShape.keys = new Keyframe[value.keys.Length];
				value.keys.CopyTo(m_lastShape.keys, 0);
				m_lastShape.preWrapMode = value.preWrapMode;
				m_lastShape.postWrapMode = value.postWrapMode;
				m_shape = value;
			}
		}

		protected override string meshName => "Path";


		protected override void Reset()
		{
			base.Reset();
		}


		protected override void BuildMesh()
		{
			base.BuildMesh();
			GenerateVertices();
			MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, m_slices, sampleCount, false);
		}


		private void GenerateVertices()
		{
			int vertexCount = (m_slices + 1) * sampleCount;
			AllocateMesh(vertexCount, m_slices * (sampleCount - 1) * 6);
			var vertexIndex = 0;

			ResetUvdistance();

			bool hasOffset = offset != Vector3.zero;
			for (var i = 0; i < sampleCount; i++)
			{
				if (m_compensateCorners)
				{
					GetSampleWithAngleCompensation(i, ref m_evalResult);
				}
				else
				{
					GetSample(i, ref m_evalResult);
				}

				Vector3 center = Vector3.zero;
				try
				{
					center = m_evalResult.position;
				}
				catch (Exception ex)
				{
					Debug.Log(ex.Message + " for i = " + i);
					return;
				}

				Vector3 right = m_evalResult.right;
				float resultSize = GetBaseSize(m_evalResult);
				if (hasOffset)
				{
					center += offset.x * resultSize * right + offset.y * resultSize * m_evalResult.up +
					          offset.z * resultSize * m_evalResult.forward;
				}

				float fullSize = size * resultSize;
				Vector3 lastVertPos = Vector3.zero;
				Quaternion rot = Quaternion.AngleAxis(rotation, m_evalResult.forward);
				if (uvMode == Uvmode.UniformClamp || uvMode == Uvmode.UniformClip)
				{
					AddUvdistance(i);
				}

				Color vertexColor = GetBaseColor(m_evalResult) * color;
				for (var n = 0; n < m_slices + 1; n++)
				{
					float slicePercent = (float)n / m_slices;
					var shapeEval = 0f;
					if (m_useShapeCurve)
					{
						shapeEval = m_shape.Evaluate(slicePercent);
					}

					tsMesh.vertices[vertexIndex] = center + rot * right * (fullSize * 0.5f) -
						rot * right * (fullSize * slicePercent) + rot * m_evalResult.up * (shapeEval * m_shapeExposure);
					CalculateUvs(m_evalResult.percent, 1f - slicePercent);
					tsMesh.uv[vertexIndex] = Vector2.one * 0.5f +
					                         (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                                   (Vector2.one * 0.5f - s_uvs));
					if (m_slices > 1)
					{
						if (n < m_slices)
						{
							float forwardPercent = (float)(n + 1) / m_slices;
							shapeEval = 0f;
							if (m_useShapeCurve)
							{
								shapeEval = m_shape.Evaluate(forwardPercent);
							}

							Vector3 nextVertPos = center + rot * right * fullSize * 0.5f -
							                      rot * right * fullSize * forwardPercent +
							                      rot * m_evalResult.up * shapeEval * m_shapeExposure;
							Vector3 cross1 = -Vector3
								.Cross(m_evalResult.forward, nextVertPos - tsMesh.vertices[vertexIndex]).normalized;

							if (n > 0)
							{
								Vector3 cross2 = -Vector3.Cross(m_evalResult.forward,
									tsMesh.vertices[vertexIndex] - lastVertPos).normalized;
								tsMesh.normals[vertexIndex] = Vector3.Slerp(cross1, cross2, 0.5f);
							}
							else
							{
								tsMesh.normals[vertexIndex] = cross1;
							}
						}
						else
						{
							tsMesh.normals[vertexIndex] = -Vector3
								.Cross(m_evalResult.forward, tsMesh.vertices[vertexIndex] - lastVertPos).normalized;
						}
					}
					else
					{
						tsMesh.normals[vertexIndex] = m_evalResult.up;
						if (rotation != 0f)
						{
							tsMesh.normals[vertexIndex] = rot * tsMesh.normals[vertexIndex];
						}
					}

					tsMesh.colors[vertexIndex] = vertexColor;
					lastVertPos = tsMesh.vertices[vertexIndex];
					vertexIndex++;
				}
			}
		}
	}
}
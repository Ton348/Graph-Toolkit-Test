using UnityEngine;

namespace Dreamteck.Splines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Dreamteck/Splines/Complex Surface Generator")]
	public class ComplexSurfaceGenerator : MeshGenerator
	{
		public enum SubdivisionMode
		{
			CatmullRom,
			BSpline,
			Linear
		}

		public enum UvwrapMode
		{
			Clamp,
			UniformX,
			UniformY,
			Uniform
		}

		[SerializeField]
		[HideInInspector]
		private UvwrapMode m_uvWrapMode = UvwrapMode.Clamp;

		[SerializeField]
		[HideInInspector]
		[Min(1)]
		private int m_subdivisions = 3;

		[SerializeField]
		[HideInInspector]
		private SubdivisionMode m_subdivisionMode;

		[SerializeField]
		[HideInInspector]
		private bool m_automaticNormals = true;

		[SerializeField]
		[HideInInspector]
		private bool m_separateMaterialIds;

		[SerializeField]
		[HideInInspector]
		private SplineComputer[] m_otherComputers = new SplineComputer[0];

		[SerializeField]
		[HideInInspector]
		private Spline[] m_splines = new Spline[0];

		[SerializeField]
		[HideInInspector]
		private bool m_initializedInEditor;

		public UvwrapMode uvWrapMode
		{
			get => m_uvWrapMode;
			set
			{
				if (value != m_uvWrapMode)
				{
					m_uvWrapMode = value;
					Rebuild();
				}
			}
		}

		public int subdivisions
		{
			get => m_subdivisions;
			set
			{
				if (value != m_subdivisions)
				{
					m_subdivisions = value;
					Rebuild();
				}
			}
		}

		public SubdivisionMode subdivisionMode
		{
			get => m_subdivisionMode;
			set
			{
				if (value != m_subdivisionMode)
				{
					m_subdivisionMode = value;
					Rebuild();
				}
			}
		}

		public bool automaticNormals
		{
			get => m_automaticNormals;
			set
			{
				if (value != m_automaticNormals)
				{
					m_automaticNormals = value;
					Rebuild();
				}
			}
		}

		public bool separateMaterialIDs
		{
			get => m_separateMaterialIds;
			set
			{
				if (value != m_separateMaterialIds)
				{
					m_separateMaterialIds = value;
					Rebuild();
				}
			}
		}


		public SplineComputer[] otherComputers
		{
			get => m_otherComputers;
			set
			{
				var rebuild = false;
				if (value.Length != m_otherComputers.Length)
				{
					rebuild = true;
					for (var i = 0; i < m_otherComputers.Length; i++)
					{
						if (m_otherComputers[i] != null)
						{
							m_otherComputers[i].Unsubscribe(this);
						}
					}
				}
				else
				{
					for (var i = 0; i < value.Length; i++)
					{
						if (m_otherComputers[i] != null)
						{
							m_otherComputers[i].Unsubscribe(this);
						}

						if (value[i] != m_otherComputers[i])
						{
							rebuild = true;
							break;
						}
					}
				}

				if (rebuild)
				{
					m_otherComputers = value;
					for (var i = 0; i < m_otherComputers.Length; i++)
					{
						if (m_otherComputers[i] != null)
						{
							if (m_otherComputers[i].subscriberCount == 0)
							{
								m_otherComputers[i].name = "Surface Spline " + (i + 1);
							}

							m_otherComputers[i].Subscribe(this);
						}
					}

					Rebuild();
				}
			}
		}

		private int iterations => m_subdivisions * m_otherComputers.Length;

		protected override void Awake()
		{
			base.Awake();

			m_mesh.name = "multispline_surface";
			for (var i = 0; i < m_otherComputers.Length; i++)
			{
				m_otherComputers[i].onRebuild -= OnOtherRebuild;
				m_otherComputers[i].onRebuild += OnOtherRebuild;
			}
		}

		protected override void Reset()
		{
			base.Reset();
		}

		private void OnOtherRebuild()
		{
			RebuildImmediate();
		}

		private Spline.Type ModeToSplineType(SubdivisionMode mode)
		{
			switch (mode)
			{
				case SubdivisionMode.BSpline: return Spline.Type.BSpline;
				case SubdivisionMode.Linear: return Spline.Type.Linear;
				default: return Spline.Type.CatmullRom;
			}
		}


		protected override void BuildMesh()
		{
			if (sampleCount == 0 || m_otherComputers.Length == 0)
			{
				AllocateMesh(0, 0);
				return;
			}

			if (m_splines.Length != sampleCount)
			{
				m_splines = new Spline[sampleCount];
				for (var i = 0; i < m_splines.Length; i++)
				{
					m_splines[i] = new Spline(ModeToSplineType(m_subdivisionMode));
				}
			}
			else
			{
				for (var i = 0; i < m_splines.Length; i++)
				{
					m_splines[i].type = ModeToSplineType(m_subdivisionMode);
				}
			}

			base.BuildMesh();
			AllocateMesh(sampleCount * (iterations + 1), iterations * (sampleCount - 1) * 6);
			tsMesh.triangles = MeshUtility.GeneratePlaneTriangles(sampleCount - 1, iterations + 1, false);
			GenerateVertices();
			tsMesh.subMeshes.Clear();

			if (m_separateMaterialIds)
			{
				for (var i = 0; i < m_otherComputers.Length; i++)
				{
					int[] newTris = MeshUtility.GeneratePlaneTriangles(sampleCount - 1, subdivisions + 1, false);
					tsMesh.subMeshes.Add(newTris);
					for (var n = 0; n < tsMesh.subMeshes[i].Length; n++)
					{
						tsMesh.subMeshes[i][n] += i * m_subdivisions * sampleCount;
					}
				}
			}
		}


		private void GenerateVertices()
		{
			if (m_otherComputers.Length == 0)
			{
				return;
			}

			ResetUvdistance();

			SplineSample sample = default;
			SplineSample sample2 = default;

			for (var i = 0; i < m_otherComputers.Length + 1; i++)
			{
				SplineComputer splineComp = spline;
				if (i > 0)
				{
					splineComp = m_otherComputers[i - 1];
				}

				for (var j = 0; j < sampleCount; j++)
				{
					if (m_splines[j].points.Length != m_otherComputers.Length + 1)
					{
						m_splines[j].points = new SplinePoint[m_otherComputers.Length + 1];
					}

					double xPercent = Dmath.Lerp(clipFrom, clipTo, (double)j / (sampleCount - 1));
					if (i > 0)
					{
						splineComp.Evaluate(xPercent, ref sample);
					}
					else
					{
						GetSample(j, ref sample);
					}

					m_splines[j].points[i].position = sample.position;
					m_splines[j].points[i].normal = sample.up;
					m_splines[j].points[i].color = sample.color;
				}
			}


			for (var x = 0; x < m_splines.Length; x++)
			{
				if (uvMode == Uvmode.UniformClamp || uvMode == Uvmode.UniformClip)
				{
					AddUvdistance(x);
				}
				else
				{
					GetSample(x, ref sample2);
				}

				Vector3 lastPos = sample.position;
				var ydist = 0f;
				float xPercent = Mathf.Lerp((float)clipFrom, (float)clipTo, (float)x / (m_splines.Length - 1));
				for (var y = 0; y < iterations + 1; y++)
				{
					float yPercent = (float)y / iterations;
					int index = x + y * m_splines.Length;
					m_splines[x].Evaluate(yPercent, ref sample);
					if (y > 0)
					{
						ydist += Vector3.Distance(lastPos, sample.position);
					}

					lastPos = sample.position;
					if (uvMode == Uvmode.UniformClamp)
					{
						s_uvs.x = CalculateUvuniformClamp(m_vDist);
						s_uvs.y = CalculateUvuniformClamp(ydist);
					}
					else if (uvMode == Uvmode.UniformClip)
					{
						s_uvs.x = CalculateUvuniformClip(m_vDist);
						s_uvs.y = CalculateUvuniformClip(ydist);
					}
					else
					{
						CalculateUvs(xPercent, yPercent);
					}

					tsMesh.vertices[index] = sample.position;
					tsMesh.normals[index] = sample.up;
					tsMesh.colors[index] = sample.color;
					tsMesh.uv[index] = Vector2.one * 0.5f +
					                   (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                             (Vector2.one * 0.5f - s_uvs));
				}
			}
		}


		protected override void WriteMesh()
		{
			base.WriteMesh();
			if (m_automaticNormals)
			{
				m_mesh.RecalculateNormals();
			}
		}

		public static void DrawSpline(Spline spline, Color color, double from = 0.0, double to = 1.0)
		{
			double add = spline.moveStep;
			int iterations = spline.iterations;
			if (iterations <= 0)
			{
				return;
			}

			Vector3 prevPoint = spline.EvaluatePosition(from);
			for (var i = 1; i < iterations; i++)
			{
				double p = Dmath.Lerp(from, to, (double)i / (iterations - 1));
				Debug.DrawLine(prevPoint, spline.EvaluatePosition(p), color, 1f);
				prevPoint = spline.EvaluatePosition(p);
			}
		}
	}
}
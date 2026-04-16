using UnityEngine;

namespace Dreamteck.Splines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Dreamteck/Splines/Users/Tube Generator")]
	public class TubeGenerator : MeshGenerator
	{
		public enum CapMethod
		{
			None,
			Flat,
			Round
		}

		[SerializeField]
		[HideInInspector]
		private int m_sides = 12;

		[SerializeField]
		[HideInInspector]
		private int m_roundCapLatitude = 6;

		[SerializeField]
		[HideInInspector]
		private CapMethod m_capMode = CapMethod.None;

		[SerializeField]
		[HideInInspector]
		[Range(0f, 360f)]
		private float m_revolve = 360f;

		[SerializeField]
		[HideInInspector]
		private float m_capUvscale = 1f;

		[SerializeField]
		[HideInInspector]
		private float m_uvTwist;

		private int m_bodyTrisCount;

		private int m_bodyVertexCount;
		private int m_capTrisCount;
		private int m_capVertexCount;

		public int sides
		{
			get => m_sides;
			set
			{
				if (value != m_sides)
				{
					if (value < 3)
					{
						value = 3;
					}

					m_sides = value;
					Rebuild();
				}
			}
		}

		public CapMethod capMode
		{
			get => m_capMode;
			set
			{
				if (value != m_capMode)
				{
					m_capMode = value;
					Rebuild();
				}
			}
		}

		public int roundCapLatitude
		{
			get => m_roundCapLatitude;
			set
			{
				if (value < 1)
				{
					value = 1;
				}

				if (value != m_roundCapLatitude)
				{
					m_roundCapLatitude = value;
					if (m_capMode == CapMethod.Round)
					{
						Rebuild();
					}
				}
			}
		}

		public float revolve
		{
			get => m_revolve;
			set
			{
				if (value != m_revolve)
				{
					m_revolve = value;
					Rebuild();
				}
			}
		}

		public float capUVScale
		{
			get => m_capUvscale;
			set
			{
				if (value != m_capUvscale)
				{
					m_capUvscale = value;
					Rebuild();
				}
			}
		}

		public float uvTwist
		{
			get => m_uvTwist;
			set
			{
				if (value != m_uvTwist)
				{
					m_uvTwist = value;
					Rebuild();
				}
			}
		}

		private bool useCap
		{
			get
			{
				bool isCapSet = m_capMode != CapMethod.None;
				if (spline != null)
				{
					return isCapSet && (!spline.isClosed || span < 1f);
				}

				return isCapSet;
			}
		}

		protected override string meshName => "Tube";

		protected override void Reset()
		{
			base.Reset();
		}

		protected override void BuildMesh()
		{
			if (m_sides <= 2)
			{
				return;
			}

			base.BuildMesh();
			m_bodyVertexCount = (m_sides + 1) * sampleCount;
			CapMethod _capModeFinal = m_capMode;
			if (!useCap)
			{
				_capModeFinal = CapMethod.None;
			}

			switch (_capModeFinal)
			{
				case CapMethod.Flat: m_capVertexCount = m_sides + 1; break;
				case CapMethod.Round: m_capVertexCount = m_roundCapLatitude * (sides + 1); break;
				default: m_capVertexCount = 0; break;
			}

			int vertexCount = m_bodyVertexCount + m_capVertexCount * 2;

			m_bodyTrisCount = m_sides * (sampleCount - 1) * 2 * 3;
			switch (_capModeFinal)
			{
				case CapMethod.Flat: m_capTrisCount = (m_sides - 1) * 3 * 2; break;
				case CapMethod.Round: m_capTrisCount = m_sides * m_roundCapLatitude * 6; break;
				default: m_capTrisCount = 0; break;
			}

			AllocateMesh(vertexCount, m_bodyTrisCount + m_capTrisCount * 2);

			Generate();
			switch (_capModeFinal)
			{
				case CapMethod.Flat: GenerateFlatCaps(); break;
				case CapMethod.Round: GenerateRoundCaps(); break;
			}
		}

		private void Generate()
		{
			var vertexIndex = 0;
			ResetUvdistance();
			bool hasOffset = offset != Vector3.zero;
			for (var i = 0; i < sampleCount; i++)
			{
				GetSample(i, ref m_evalResult);
				Vector3 center = m_evalResult.position;
				Vector3 right = m_evalResult.right;
				float resultSize = GetBaseSize(m_evalResult);
				if (hasOffset)
				{
					center += offset.x * resultSize * right + offset.y * resultSize * m_evalResult.up +
					          offset.z * resultSize * m_evalResult.forward;
				}

				if (uvMode == Uvmode.UniformClamp || uvMode == Uvmode.UniformClip)
				{
					AddUvdistance(i);
				}

				Color vertexColor = GetBaseColor(m_evalResult) * color;
				for (var n = 0; n < m_sides + 1; n++)
				{
					float anglePercent = (float)n / m_sides;
					Quaternion rot = Quaternion.AngleAxis(m_revolve * anglePercent + rotation + 180f,
						m_evalResult.forward);
					tsMesh.vertices[vertexIndex] = center + rot * right * (size * resultSize * 0.5f);
					CalculateUvs(m_evalResult.percent, anglePercent);
					tsMesh.uv[vertexIndex] = Vector2.one * 0.5f +
					                         (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                                   (Vector2.one * 0.5f - (s_uvs + Vector2.right *
						                                   ((float)m_evalResult.percent * m_uvTwist))));
					tsMesh.normals[vertexIndex] = Vector3.Normalize(tsMesh.vertices[vertexIndex] - center);
					tsMesh.colors[vertexIndex] = vertexColor;
					vertexIndex++;
				}
			}

			MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, m_sides, sampleCount, false);
		}

		private void GenerateFlatCaps()
		{
			//Start Cap

			GetSample(0, ref m_evalResult);
			for (var i = 0; i < m_sides + 1; i++)
			{
				int index = m_bodyVertexCount + i;
				tsMesh.vertices[index] = tsMesh.vertices[i];
				tsMesh.normals[index] = -m_evalResult.forward;
				tsMesh.colors[index] = tsMesh.colors[i];
				tsMesh.uv[index] =
					Quaternion.AngleAxis(m_revolve * ((float)i / (m_sides - 1)), Vector3.forward) * Vector2.right *
					(0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
			}

			//End Cap
			GetSample(sampleCount - 1, ref m_evalResult);
			for (var i = 0; i < m_sides + 1; i++)
			{
				int index = m_bodyVertexCount + m_sides + 1 + i;
				int bodyIndex = m_bodyVertexCount - (m_sides + 1) + i;
				tsMesh.vertices[index] = tsMesh.vertices[bodyIndex];
				tsMesh.normals[index] = m_evalResult.forward;
				tsMesh.colors[index] = tsMesh.colors[bodyIndex];
				tsMesh.uv[index] =
					Quaternion.AngleAxis(m_revolve * ((float)bodyIndex / (m_sides - 1)), Vector3.forward) *
					Vector2.right * (0.5f * capUVScale) + Vector3.right * 0.5f + Vector3.up * 0.5f;
			}

			int t = m_bodyTrisCount;
			bool fullIntegrity = m_revolve == 360f;
			int finalSides = fullIntegrity ? m_sides - 1 : m_sides;
			//Start cap
			for (var i = 0; i < finalSides - 1; i++)
			{
				tsMesh.triangles[t++] = i + m_bodyVertexCount + 2;
				tsMesh.triangles[t++] = i + +m_bodyVertexCount + 1;
				tsMesh.triangles[t++] = m_bodyVertexCount;
			}

			//End cap
			for (var i = 0; i < finalSides - 1; i++)
			{
				tsMesh.triangles[t++] = m_bodyVertexCount + m_sides + 1;
				tsMesh.triangles[t++] = i + 1 + m_bodyVertexCount + m_sides + 1;
				tsMesh.triangles[t++] = i + 2 + m_bodyVertexCount + m_sides + 1;
			}
		}

		private void GenerateRoundCaps()
		{
			//Start Cap
			GetSample(0, ref m_evalResult);
			Vector3 center = m_evalResult.position;
			bool hasOffset = offset != Vector3.zero;
			float resultSize = GetBaseSize(m_evalResult);
			if (hasOffset)
			{
				center += offset.x * resultSize * m_evalResult.right + offset.y * resultSize * m_evalResult.up +
				          offset.z * resultSize * m_evalResult.forward;
			}

			Quaternion lookRot = Quaternion.LookRotation(-m_evalResult.forward, m_evalResult.up);
			var startV = 0f;
			var capLengthPercent = 0f;
			switch (uvMode)
			{
				case Uvmode.Clip:
					startV = (float)m_evalResult.percent;
					capLengthPercent = size * 0.5f / spline.CalculateLength();
					break;
				case Uvmode.UniformClip:
					startV = spline.CalculateLength(0.0, m_evalResult.percent);
					capLengthPercent = size * 0.5f;
					break;
				case Uvmode.UniformClamp:
					startV = 0f;
					capLengthPercent = size * 0.5f / (float)span;
					break;
				case Uvmode.Clamp: capLengthPercent = size * 0.5f / spline.CalculateLength(clipFrom, clipTo); break;
			}

			Color vertexColor = GetBaseColor(m_evalResult) * color;
			for (var lat = 1; lat < m_roundCapLatitude + 1; lat++)
			{
				float latitudePercent = (float)lat / m_roundCapLatitude;
				float latAngle = 90f * latitudePercent;
				for (var lon = 0; lon <= sides; lon++)
				{
					float anglePercent = (float)lon / sides;
					int index = m_bodyVertexCount + lon + (lat - 1) * (sides + 1);
					Quaternion rot =
						Quaternion.AngleAxis(m_revolve * anglePercent + rotation + 180f, -Vector3.forward) *
						Quaternion.AngleAxis(latAngle, Vector3.up);
					tsMesh.vertices[index] =
						center + lookRot * rot * -Vector3.right * (size * 0.5f * m_evalResult.size);
					tsMesh.colors[index] = vertexColor;
					tsMesh.normals[index] = (tsMesh.vertices[index] - center).normalized;
					float baseV = startV + capLengthPercent * latitudePercent;
					Vector2 baseUV = new Vector2(anglePercent * uvScale.x - baseV * m_uvTwist, baseV * uvScale.y) -
					                 uvOffset;
					tsMesh.uv[index] = Vector2.one * 0.5f +
					                   (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                             (Vector2.one * 0.5f - baseUV));
				}
			}


			//Triangles
			int t = m_bodyTrisCount;
			for (int z = -1; z < m_roundCapLatitude - 1; z++)
			{
				for (var x = 0; x < sides; x++)
				{
					int current = m_bodyVertexCount + x + z * (sides + 1);
					int next = current + sides + 1;
					if (z == -1)
					{
						current = x;
						next = m_bodyVertexCount + x;
					}

					tsMesh.triangles[t++] = next + 1;
					tsMesh.triangles[t++] = current + 1;
					tsMesh.triangles[t++] = current;
					tsMesh.triangles[t++] = next;
					tsMesh.triangles[t++] = next + 1;
					tsMesh.triangles[t++] = current;
				}
			}


			//End Cap
			GetSample(sampleCount - 1, ref m_evalResult);
			center = m_evalResult.position;
			resultSize = GetBaseSize(m_evalResult);
			if (hasOffset)
			{
				center += offset.x * resultSize * m_evalResult.right + offset.y * resultSize * m_evalResult.up +
				          offset.z * resultSize * m_evalResult.forward;
			}

			lookRot = Quaternion.LookRotation(m_evalResult.forward, m_evalResult.up);
			switch (uvMode)
			{
				case Uvmode.Clip: startV = (float)m_evalResult.percent; break;
				case Uvmode.UniformClip: startV = spline.CalculateLength(0.0, m_evalResult.percent); break;
				case Uvmode.Clamp: startV = 1f; break;
				case Uvmode.UniformClamp: startV = spline.CalculateLength(); break;
			}

			vertexColor = GetBaseColor(m_evalResult) * color;
			for (var lat = 1; lat < m_roundCapLatitude + 1; lat++)
			{
				float latitudePercent = (float)lat / m_roundCapLatitude;
				float latAngle = 90f * latitudePercent;
				for (var lon = 0; lon <= sides; lon++)
				{
					float anglePercent = (float)lon / sides;
					int index = m_bodyVertexCount + m_capVertexCount + lon + (lat - 1) * (sides + 1);
					Quaternion rot = Quaternion.AngleAxis(m_revolve * anglePercent + rotation + 180f, Vector3.forward) *
					                 Quaternion.AngleAxis(latAngle, -Vector3.up);
					tsMesh.vertices[index] = center + lookRot * rot * Vector3.right * size * 0.5f * m_evalResult.size;
					tsMesh.normals[index] = (tsMesh.vertices[index] - center).normalized;
					tsMesh.colors[index] = vertexColor;
					float baseV = startV + capLengthPercent * latitudePercent;
					Vector2 baseUV = new Vector2(anglePercent * uvScale.x + baseV * m_uvTwist, baseV * uvScale.y) -
					                 uvOffset;
					tsMesh.uv[index] = Vector2.one * 0.5f +
					                   (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) *
					                             (Vector2.one * 0.5f - baseUV));
				}
			}

			//Triangles
			for (int z = -1; z < m_roundCapLatitude - 1; z++)
			{
				for (var x = 0; x < sides; x++)
				{
					int current = m_bodyVertexCount + m_capVertexCount + x + z * (sides + 1);
					int next = current + sides + 1;
					if (z == -1)
					{
						current = m_bodyVertexCount - (m_sides + 1) + x;
						next = m_bodyVertexCount + m_capVertexCount + x;
					}

					tsMesh.triangles[t++] = current + 1;
					tsMesh.triangles[t++] = next + 1;
					tsMesh.triangles[t++] = next;
					tsMesh.triangles[t++] = next;
					tsMesh.triangles[t++] = current;
					tsMesh.triangles[t++] = current + 1;
				}
			}
		}
	}
}
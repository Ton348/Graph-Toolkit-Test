using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Dreamteck.Splines.IO
{
	public class SplineParser
	{
		private readonly CultureInfo m_culture = new("en-US");
		private readonly NumberStyles m_style = NumberStyles.Any;

		internal SplineDefinition buffer = null;
		protected string m_fileName = "";
		public string name => m_fileName;

		internal Vector2[] ParseVector2(string coord)
		{
			List<float> list = ParseFloatArray(coord.Substring(1));
			int count = list.Count / 2;
			if (count == 0)
			{
				Debug.Log("Error in " + coord);
				return new[] { Vector2.zero };
			}

			var vectors = new Vector2[count];
			for (var i = 0; i < count; i++)
			{
				vectors[i] = new Vector2(list[0 + i * 2], -list[1 + i * 2]);
			}

			return vectors;
		}

		internal float[] ParseFloat(string coord)
		{
			List<float> list = ParseFloatArray(coord.Substring(1));
			if (list.Count < 1)
			{
				Debug.Log("Error in " + coord);
				return new[] { 0f };
			}

			return list.ToArray();
		}

		internal List<float> ParseFloatArray(string content)
		{
			var accumulated = "";
			var list = new List<float>();
			foreach (char c in content)
			{
				if (c == ',' || c == '-' || char.IsWhiteSpace(c) || (accumulated.Contains(".") && c == '.'))
				{
					if (!IsWhiteSpace(accumulated))
					{
						var parsed = 0f;
						float.TryParse(accumulated, m_style, m_culture, out parsed);
						list.Add(parsed);
						accumulated = "";
						if (c == '-')
						{
							accumulated = "-";
						}

						if (c == '.')
						{
							accumulated = "0.";
						}

						continue;
					}
				}

				if (!char.IsWhiteSpace(c))
				{
					accumulated += c;
				}
			}

			if (!IsWhiteSpace(accumulated))
			{
				var p = 0f;
				float.TryParse(accumulated, m_style, m_culture, out p);
				list.Add(p);
			}

			return list;
		}

		public bool IsWhiteSpace(string s)
		{
			foreach (char c in s)
			{
				if (!char.IsWhiteSpace(c))
				{
					return false;
				}
			}

			return true;
		}

		internal class Transformation
		{
			protected static Matrix4x4 s_matrix;

			internal static void ResetMatrix()
			{
				s_matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
			}

			internal virtual void Push()
			{
			}

			internal static void Apply(SplinePoint[] points)
			{
				for (var i = 0; i < points.Length; i++)
				{
					SplinePoint p = points[i];
					p.position = s_matrix.MultiplyPoint(p.position);
					p.tangent = s_matrix.MultiplyPoint(p.tangent);
					p.tangent2 = s_matrix.MultiplyPoint(p.tangent2);
					points[i] = p;
				}
			}
		}

		internal class Translate : Transformation
		{
			private readonly Vector2 m_offset = Vector2.zero;

			public Translate(Vector2 o)
			{
				m_offset = o;
			}

			internal override void Push()
			{
				var translate = new Matrix4x4();
				translate.SetTRS(new Vector2(m_offset.x, -m_offset.y), Quaternion.identity, Vector3.one);
				s_matrix = s_matrix * translate;
			}
		}

		internal class Rotate : Transformation
		{
			private readonly float m_angle;

			public Rotate(float a)
			{
				m_angle = a;
			}

			internal override void Push()
			{
				var rotate = new Matrix4x4();
				rotate.SetTRS(Vector3.zero, Quaternion.AngleAxis(m_angle, Vector3.back), Vector3.one);
				s_matrix = s_matrix * rotate;
			}
		}

		internal class Scale : Transformation
		{
			private readonly Vector2 m_multiplier = Vector2.one;

			public Scale(Vector2 s)
			{
				m_multiplier = s;
			}

			internal override void Push()
			{
				var scale = new Matrix4x4();
				scale.SetTRS(Vector3.zero, Quaternion.identity, m_multiplier);
				s_matrix = s_matrix * scale;
			}
		}

		internal class SkewX : Transformation
		{
			private readonly float m_amount;

			public SkewX(float a)
			{
				m_amount = a;
			}

			internal override void Push()
			{
				var skew = new Matrix4x4();
				skew[0, 0] = 1.0f;
				skew[1, 1] = 1.0f;
				skew[2, 2] = 1.0f;
				skew[3, 3] = 1.0f;
				skew[0, 1] = Mathf.Tan(-m_amount * Mathf.Deg2Rad);
				s_matrix = s_matrix * skew;
			}
		}

		internal class SkewY : Transformation
		{
			private readonly float m_amount;

			public SkewY(float a)
			{
				m_amount = a;
			}

			internal override void Push()
			{
				var skew = new Matrix4x4();
				skew[0, 0] = 1.0f;
				skew[1, 1] = 1.0f;
				skew[2, 2] = 1.0f;
				skew[3, 3] = 1.0f;
				skew[1, 0] = Mathf.Tan(-m_amount * Mathf.Deg2Rad);
				s_matrix = s_matrix * skew;
			}
		}

		internal class MatrixTransform : Transformation
		{
			private readonly Matrix4x4 m_transformMatrix;

			public MatrixTransform(float a, float b, float c, float d, float e, float f)
			{
				m_transformMatrix.SetRow(0, new Vector4(a, c, 0f, e));
				m_transformMatrix.SetRow(1, new Vector4(b, d, 0f, -f));
				m_transformMatrix.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
				m_transformMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			}

			internal override void Push()
			{
				s_matrix = s_matrix * m_transformMatrix;
			}
		}


		internal class SplineDefinition
		{
			internal bool closed;
			internal Color color = Color.white;
			internal string name = "";
			internal Vector3 normal = Vector3.back;
			internal List<SplinePoint> points = new();

			internal Vector3 position = Vector3.zero;
			internal float size = 1f;
			internal Vector3 tangent = Vector3.zero;
			internal Vector3 tangent2 = Vector3.zero;
			internal Spline.Type type = Spline.Type.Linear;

			internal SplineDefinition(string n, Spline.Type t)
			{
				name = n;
				type = t;
			}

			internal SplineDefinition(string n, Spline spline)
			{
				name = n;
				type = spline.type;
				closed = spline.isClosed;
				points = new List<SplinePoint>(spline.points);
			}

			internal int pointCount => points.Count;

			internal SplinePoint GetLastPoint()
			{
				if (points.Count == 0)
				{
					return new SplinePoint();
				}

				return points[points.Count - 1];
			}

			internal void SetLastPoint(SplinePoint point)
			{
				if (points.Count == 0)
				{
					return;
				}

				points[points.Count - 1] = point;
			}

			internal void CreateClosingPoint()
			{
				var p = new SplinePoint(points[0]);
				points.Add(p);
			}

			internal void CreateSmooth()
			{
				points.Add(new SplinePoint(position, tangent, normal, size, color));
			}

			internal void CreateBroken()
			{
				var point = new SplinePoint(new SplinePoint(position, tangent, normal, size, color));
				point.type = SplinePoint.Type.Broken;
				point.SetTangent2Position(point.position);
				point.normal = normal;
				point.color = color;
				point.size = size;
				points.Add(point);
			}

			internal void CreateLinear()
			{
				tangent = position;
				CreateSmooth();
			}

			internal SplineComputer CreateSplineComputer(Vector3 position, Quaternion rotation)
			{
				var go = new GameObject(name);
				go.transform.position = position;
				go.transform.rotation = rotation;
				var computer = go.AddComponent<SplineComputer>();
#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					computer.ResampleTransform();
				}
#endif
				computer.type = type;
				if (closed)
				{
					if (points[0].type == SplinePoint.Type.Broken)
					{
						points[0].SetTangentPosition(GetLastPoint().tangent2);
					}
				}

				computer.SetPoints(points.ToArray(), SplineComputer.Space.Local);
				if (closed)
				{
					computer.Close();
				}

				return computer;
			}

			internal Spline CreateSpline()
			{
				var spline = new Spline(type);
				spline.points = points.ToArray();
				if (closed)
				{
					spline.Close();
				}

				return spline;
			}

			internal void Transform(List<Transformation> trs)
			{
				SplinePoint[] p = points.ToArray();
				Transformation.ResetMatrix();
				foreach (Transformation t in trs)
				{
					t.Push();
				}

				Transformation.Apply(p);
				for (var i = 0; i < p.Length; i++)
				{
					points[i] = p[i];
				}

				var debugPoints = new SplinePoint[1];
				debugPoints[0] = new SplinePoint();
				Transformation.Apply(debugPoints);
			}
		}
	}
}
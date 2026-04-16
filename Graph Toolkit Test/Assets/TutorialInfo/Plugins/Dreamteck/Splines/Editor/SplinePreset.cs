using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[Serializable]
	public struct S_Vector3
	{
		public float x, y, z;

		public Vector3 vector
		{
			get => new(x, y, z);
			set { }
		}


		public S_Vector3(Vector3 input)
		{
			x = input.x;
			y = input.y;
			z = input.z;
		}
	}

	[Serializable]
	public struct S_Color
	{
		public float r, g, b, a;

		public Color color
		{
			get => new(r, g, b, a);
			set { }
		}

		public S_Color(Color input)
		{
			r = input.r;
			g = input.g;
			b = input.b;
			a = input.a;
		}
	}

	[Serializable]
	public class SplinePreset
	{
		private static string s_path = "";

		[SerializeField]
		private S_Vector3[] m_pointsPosition = new S_Vector3[0];

		[SerializeField]
		private S_Vector3[] m_pointsTanget = new S_Vector3[0];

		[SerializeField]
		private S_Vector3[] m_pointsTangent2 = new S_Vector3[0];

		[SerializeField]
		private S_Vector3[] m_pointsNormal = new S_Vector3[0];

		[SerializeField]
		private S_Color[] m_pointsColor = new S_Color[0];

		[SerializeField]
		private float[] m_pointsSize = new float[0];

		[SerializeField]
		private SplinePoint.Type[] m_pointsType = new SplinePoint.Type[0];

		public bool isClosed;
		public string filename = "";
		public string name = "";
		public string description = "";
		public Spline.Type type = Spline.Type.Bezier;


		[NonSerialized]
		protected SplineComputer m_computer;

		[NonSerialized]
		public Vector3 origin = Vector3.zero;

		public SplinePreset(SerializedSplinePoint[] p, bool closed, Spline.Type t)
		{
			m_pointsPosition = new S_Vector3[p.Length];
			m_pointsTanget = new S_Vector3[p.Length];
			m_pointsTangent2 = new S_Vector3[p.Length];
			m_pointsNormal = new S_Vector3[p.Length];
			m_pointsColor = new S_Color[p.Length];
			m_pointsSize = new float[p.Length];
			m_pointsType = new SplinePoint.Type[p.Length];
			for (var i = 0; i < p.Length; i++)
			{
				m_pointsPosition[i] = new S_Vector3(p[i].position);
				m_pointsTanget[i] = new S_Vector3(p[i].tangent);
				m_pointsTangent2[i] = new S_Vector3(p[i].tangent2);
				m_pointsNormal[i] = new S_Vector3(p[i].normal);
				m_pointsColor[i] = new S_Color(p[i].color);
				m_pointsSize[i] = p[i].size;
				m_pointsType[i] = p[i].type;
			}

			isClosed = closed;
			type = t;
			s_path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
		}

		public SplinePoint[] points
		{
			get
			{
				var p = new SplinePoint[m_pointsPosition.Length];
				for (var i = 0; i < p.Length; i++)
				{
					p[i].type = m_pointsType[i];
					p[i].position = m_pointsPosition[i].vector;
					p[i].tangent = m_pointsTanget[i].vector;
					p[i].tangent2 = m_pointsTangent2[i].vector;
					p[i].normal = m_pointsNormal[i].vector;
					p[i].color = m_pointsColor[i].color;
					p[i].size = m_pointsSize[i];
				}

				return p;
			}
		}

		public void Save(string name)
		{
			if (!Directory.Exists(s_path))
			{
				Directory.CreateDirectory(s_path);
			}

			FileStream file = File.Create(s_path + "/" + name + ".jsp");
			byte[] bytes = ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(this));
			file.Write(bytes, 0, bytes.Length);
			file.Close();
		}

		public static void Delete(string filename)
		{
			s_path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
			if (!Directory.Exists(s_path))
			{
				Debug.LogError("Directory " + s_path + " does not exist");
				return;
			}

			File.Delete(s_path + "/" + filename);
		}

		public static SplinePreset[] LoadAll()
		{
			s_path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Presets");
			if (!Directory.Exists(s_path))
			{
				Debug.LogError("Directory " + s_path + " does not exist");
				return null;
			}

			string[] files = Directory.GetFiles(s_path, "*.jsp");
			var presets = new SplinePreset[files.Length];
			for (var i = 0; i < files.Length; i++)
			{
				FileStream file = File.Open(files[i], FileMode.Open);
				var bytes = new byte[file.Length];
				file.Read(bytes, 0, bytes.Length);
				string json = ASCIIEncoding.ASCII.GetString(bytes);
				presets[i] = JsonUtility.FromJson<SplinePreset>(json);
				file.Close();
			}

			return presets;
		}
	}
}
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dreamteck.Splines.IO
{
	public class Csv : SplineParser
	{
		public enum ColumnType
		{
			Position,
			Tangent,
			Tangent2,
			Normal,
			Size,
			Color
		}

		private readonly CultureInfo m_culture = new("en-US");
		private readonly NumberStyles m_style = NumberStyles.Any;
		public List<ColumnType> columns = new();

		public Csv(SplineComputer computer)
		{
			var spline = new Spline(computer.type, computer.sampleRate);
			spline.points = computer.GetPoints();
			if (spline.type != Spline.Type.Bezier && spline.type != Spline.Type.Linear)
			{
				spline.CatToBezierTangents();
			}

			if (computer.isClosed)
			{
				spline.Close();
			}

			buffer = new SplineDefinition(computer.name, spline);
			m_fileName = computer.name;
			columns.Add(ColumnType.Position);
			columns.Add(ColumnType.Tangent);
			columns.Add(ColumnType.Tangent2);
		}

		public Csv(string filePath, List<ColumnType> customColumns = null)
		{
			if (File.Exists(filePath))
			{
				string ext = Path.GetExtension(filePath).ToLower();
				m_fileName = Path.GetFileNameWithoutExtension(filePath);
				if (ext != ".csv")
				{
					Debug.LogError("CSV Parsing ERROR: Wrong format. Please use SVG or XML");
					return;
				}

				string[] lines = File.ReadAllLines(filePath);
				if (customColumns == null)
				{
					columns.Add(ColumnType.Position);
					columns.Add(ColumnType.Tangent);
					columns.Add(ColumnType.Tangent2);
					columns.Add(ColumnType.Normal);
					columns.Add(ColumnType.Size);
					columns.Add(ColumnType.Color);
				}
				else
				{
					columns = new List<ColumnType>(customColumns);
				}

				buffer = new SplineDefinition(m_fileName, Spline.Type.CatmullRom);
				Read(lines);
			}
		}

		private void Read(string[] lines)
		{
			var expectedElementCount = 0;
			foreach (ColumnType col in columns)
			{
				switch (col)
				{
					case ColumnType.Position: expectedElementCount += 3; break;
					case ColumnType.Tangent: expectedElementCount += 3; break;
					case ColumnType.Tangent2: expectedElementCount += 3; break;
					case ColumnType.Normal: expectedElementCount += 3; break;
					case ColumnType.Size: expectedElementCount++; break;
					case ColumnType.Color: expectedElementCount += 4; break;
				}
			}

			for (var i = 1; i < lines.Length; i++)
			{
				lines[i] = Regex.Replace(lines[i], @"\s+", "");
				string[] elements = lines[i].Split(',');
				if (elements.Length != expectedElementCount)
				{
					Debug.LogError("Unexpected element count on row " + i + ". Expected " + expectedElementCount +
					               " found " + elements.Length +
					               " Please make sure that all values exist and the column order is correct.");
					continue;
				}

				var values = new float[elements.Length];
				for (var j = 0; j < elements.Length; j++)
				{
					float.TryParse(elements[j], m_style, m_culture, out values[j]);
				}

				var currentValue = 0;
				foreach (ColumnType col in columns)
				{
					switch (col)
					{
						case ColumnType.Position:
							buffer.position = new Vector3(values[currentValue++], values[currentValue++],
								values[currentValue++]); break;
						case ColumnType.Tangent:
							buffer.tangent = new Vector3(values[currentValue++], values[currentValue++],
								values[currentValue++]); break;
						case ColumnType.Tangent2:
							buffer.tangent2 = new Vector3(values[currentValue++], values[currentValue++],
								values[currentValue++]); break;
						case ColumnType.Normal:
							buffer.normal = new Vector3(values[currentValue++], values[currentValue++],
								values[currentValue++]); break;
						case ColumnType.Size: buffer.size = values[currentValue++]; break;
						case ColumnType.Color:
							buffer.color = new Color(values[currentValue++], values[currentValue++],
								values[currentValue++], values[currentValue++]); break;
					}
				}

				buffer.CreateSmooth();
			}
		}

		public SplineComputer CreateSplineComputer(Vector3 position, Quaternion rotation)
		{
			return buffer.CreateSplineComputer(position, rotation);
		}

		public Spline CreateSpline()
		{
			return buffer.CreateSpline();
		}


		public void FlatX()
		{
			for (var i = 0; i < buffer.pointCount; i++)
			{
				buffer.points[i].Flatten(LinearAlgebraUtility.Axis.X);
			}
		}

		public void FlatY()
		{
			for (var i = 0; i < buffer.pointCount; i++)
			{
				buffer.points[i].Flatten(LinearAlgebraUtility.Axis.Y);
			}
		}

		public void FlatZ()
		{
			for (var i = 0; i < buffer.pointCount; i++)
			{
				buffer.points[i].Flatten(LinearAlgebraUtility.Axis.Z);
			}
		}

		private void AddTitle(ref string[] content, string title)
		{
			if (!string.IsNullOrEmpty(content[0]))
			{
				content[0] += ",";
			}

			content[0] += title;
		}

		private void AddVector3Title(ref string[] content, string prefix)
		{
			AddTitle(ref content, prefix + "X," + prefix + "Y," + prefix + "Z");
		}

		private void AddColorTitle(ref string[] content, string prefix)
		{
			AddTitle(ref content, prefix + "R," + prefix + "G," + prefix + "B" + prefix + "A");
		}

		private void AddVector3(ref string[] content, int index, Vector3 vector)
		{
			AddFloat(ref content, index, vector.x);
			AddFloat(ref content, index, vector.y);
			AddFloat(ref content, index, vector.z);
		}

		private void AddColor(ref string[] content, int index, Color color)
		{
			AddFloat(ref content, index, color.r);
			AddFloat(ref content, index, color.g);
			AddFloat(ref content, index, color.b);
			AddFloat(ref content, index, color.a);
		}

		private void AddFloat(ref string[] content, int index, float value)
		{
			if (!string.IsNullOrEmpty(content[index]))
			{
				content[index] += ",";
			}

			content[index] += value.ToString();
		}

		public void Write(string filePath)
		{
			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			{
				throw new DirectoryNotFoundException("The file is being saved to a non-existing directory.");
			}

			List<SplinePoint> csvPoints = buffer.points;
			var content = new string[csvPoints.Count + 1];
			//Add the column titles
			foreach (ColumnType col in columns)
			{
				switch (col)
				{
					case ColumnType.Position: AddVector3Title(ref content, "Position"); break;
					case ColumnType.Tangent: AddVector3Title(ref content, "Tangent"); break;
					case ColumnType.Tangent2: AddVector3Title(ref content, "Tangent2"); break;
					case ColumnType.Normal: AddVector3Title(ref content, "Normal"); break;
					case ColumnType.Size: AddTitle(ref content, "Size"); break;
					case ColumnType.Color: AddColorTitle(ref content, "Color"); break;
				}
			}

			//Add the content for each column
			foreach (ColumnType col in columns)
			{
				for (var i = 1; i <= csvPoints.Count; i++)
				{
					int index = i - 1;
					switch (col)
					{
						case ColumnType.Position: AddVector3(ref content, i, csvPoints[index].position); break;
						case ColumnType.Tangent: AddVector3(ref content, i, csvPoints[index].tangent); break;
						case ColumnType.Tangent2: AddVector3(ref content, i, csvPoints[index].tangent2); break;
						case ColumnType.Normal: AddVector3(ref content, i, csvPoints[index].normal); break;
						case ColumnType.Size: AddFloat(ref content, i, csvPoints[index].size); break;
						case ColumnType.Color: AddColor(ref content, i, csvPoints[index].color); break;
					}
				}
			}

			File.WriteAllLines(filePath, content);
		}
	}
}
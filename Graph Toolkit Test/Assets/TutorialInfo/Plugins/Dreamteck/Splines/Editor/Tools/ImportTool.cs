using System.Collections.Generic;
using System.IO;
using Dreamteck.Splines.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class ImportExportTool : SplineTool
	{
		private readonly List<SplinePoint[]> m_originalPoints = new();
		private bool m_alwaysDraw = true;
		private Axis m_exportAxis = Axis.Z;

		private List<Csv.ColumnType> m_exportColumns = new();
		private List<SplineComputer> m_exported = new();
		private string m_exportPath = "";
		private bool m_flatCsv;
		private ExportFormat m_format = ExportFormat.SVG;
		private Axis m_importAxis = Axis.Z;
		private List<Csv.ColumnType> m_importColumns = new();
		private List<SplineComputer> m_imported = new();
		private GameObject m_importedParent;

		private bool m_importOptions;
		private string m_importPath = "";

		private Mode m_mode = Mode.None;
		private float m_scaleFactor = 1f;

		public override string GetName()
		{
			return "Import/Export";
		}

		protected override string GetPrefix()
		{
			return "ImportExport";
		}

		public override void Open(EditorWindow window)
		{
			base.Open(window);
			m_importPath = LoadString("importPath", "");
			m_exportPath = LoadString("exportPath", "");
			m_alwaysDraw = LoadBool("alwaysDraw", true);
			m_flatCsv = LoadBool("flatCSV", false);
			m_importAxis = (Axis)LoadInt("importAxis", 2);
			m_exportAxis = (Axis)LoadInt("exportAxis", 2);
			LoadColumns("importColumns", ref m_importColumns);
			LoadColumns("exportColumns", ref m_exportColumns);
		}

		private void LoadColumns(string name, ref List<Csv.ColumnType> destination)
		{
			string text = LoadString(name, "");
			destination = new List<Csv.ColumnType>();
			if (text == "")
			{
				destination.Add(Csv.ColumnType.Position);
				destination.Add(Csv.ColumnType.Tangent);
				destination.Add(Csv.ColumnType.Tangent2);
				destination.Add(Csv.ColumnType.Normal);
				destination.Add(Csv.ColumnType.Size);
				destination.Add(Csv.ColumnType.Color);
				return;
			}

			string[] elements = text.Split(',');
			foreach (string element in elements)
			{
				var i = 0;
				if (int.TryParse(element, out i))
				{
					destination.Add((Csv.ColumnType)i);
				}
			}
		}

		public override void Close()
		{
			base.Close();
			if (m_importPath != "")
			{
				SaveString("importPath", Path.GetDirectoryName(m_importPath));
			}

			if (m_exportPath != "")
			{
				SaveString("exportPath", Path.GetDirectoryName(m_exportPath));
			}

			var columnString = "";
			foreach (Csv.ColumnType col in m_importColumns)
			{
				if (columnString != "")
				{
					columnString += ",";
				}

				columnString += ((int)col).ToString();
			}

			SaveString("importColumns", columnString);
			columnString = "";
			foreach (Csv.ColumnType col in m_exportColumns)
			{
				if (columnString != "")
				{
					columnString += ",";
				}

				columnString += ((int)col).ToString();
			}

			SaveString("exportColumns", columnString);
			SaveBool("alwaysDraw", m_alwaysDraw);
			SaveBool("flatCSV", m_flatCsv);
			SaveInt("importAxis", (int)m_importAxis);
			SaveInt("exportAxis", (int)m_exportAxis);

#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
		}

		protected override void Save()
		{
			base.Save();
			if (m_importedParent != null)
			{
				Selection.activeGameObject = m_importedParent;
				m_importedParent = null;
			}
			else
			{
				foreach (SplineComputer comp in m_imported)
				{
					if (comp != null)
					{
						Selection.activeGameObject = comp.gameObject;
						break;
					}
				}
			}

			m_imported.Clear();
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

			m_mode = Mode.None;
		}

		protected override void Cancel()
		{
			base.Cancel();
			foreach (SplineComputer spline in m_imported)
			{
				GameObject.DestroyImmediate(spline.gameObject);
			}

			GameObject.DestroyImmediate(m_importedParent);
			m_imported.Clear();
			SceneView.RepaintAll();
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

			m_mode = Mode.None;
		}

		private void CsvcolumnUi(List<Csv.ColumnType> columns)
		{
			EditorGUILayout.LabelField("Dataset Columns");
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("-", GUILayout.MaxWidth(30)) && columns.Count > 0)
			{
				columns.RemoveAt(columns.Count - 1);
			}

			for (var i = 0; i < columns.Count; i++)
			{
				columns[i] = (Csv.ColumnType)EditorGUILayout.EnumPopup(columns[i]);
			}

			if (GUILayout.Button("+", GUILayout.MaxWidth(30)) && columns.Count > 0)
			{
				columns.Add(Csv.ColumnType.Position);
			}

			EditorGUILayout.EndHorizontal();
		}

		private void OnScene(SceneView current)
		{
			for (var i = 0; i < m_imported.Count; i++)
			{
				DssplineDrawer.DrawSplineComputer(m_imported[i]);
			}
		}

		private void ImportUi()
		{
			EditorGUI.BeginChangeCheck();
			m_scaleFactor = EditorGUILayout.FloatField("Scale Factor", m_scaleFactor);
			m_importAxis = (Axis)EditorGUILayout.EnumPopup("Facing Axis", m_importAxis);
			m_alwaysDraw = EditorGUILayout.Toggle("Always Draw", m_alwaysDraw);
			if (EditorGUI.EndChangeCheck())
			{
				ApplyPoints();
			}

			SaveCancelUi();
		}

		private void ExportUi()
		{
			if (m_exported.Count == 0)
			{
				m_mode = Mode.None;
				return;
			}

			EditorGUILayout.Space();
			m_format = (ExportFormat)EditorGUILayout.EnumPopup("Format", m_format);
			if (m_format == ExportFormat.SVG)
			{
				m_exportAxis = (Axis)EditorGUILayout.EnumPopup("Projection Axis", m_exportAxis);
				EditorGUILayout.HelpBox(
					"The SVG is a 2D vector format so the exported spline will be flattened along the selected axis",
					MessageType.Info);
			}
			else
			{
				CsvcolumnUi(m_exportColumns);
				m_flatCsv = EditorGUILayout.Toggle("Flat", m_flatCsv);
				if (m_flatCsv)
				{
					m_exportAxis = (Axis)EditorGUILayout.EnumPopup("Projection Axis", m_exportAxis);
				}

				EditorGUILayout.HelpBox("The exported splined will be flattened along the selected axis.",
					MessageType.Info);
			}

			if (GUILayout.Button("Save File"))
			{
				var extension = "*";
				switch (m_format)
				{
					case ExportFormat.SVG: extension = "svg"; break;
					case ExportFormat.CSV: extension = "csv"; break;
				}

				m_exportPath = EditorUtility.SaveFilePanel("Export splines", m_exportPath, "spline", extension);
				if (m_exportPath != "")
				{
					if (Directory.Exists(Path.GetDirectoryName(m_exportPath)))
					{
						switch (m_format)
						{
							case ExportFormat.SVG: ExportSvg(m_exportPath); break;
							case ExportFormat.CSV: ExportCsv(m_exportPath); break;
						}
					}
				}
			}
		}

		public override void Draw(Rect windowRect)
		{
			if (m_mode == Mode.Import)
			{
				ImportUi();
			}
			else
			{
				m_importOptions = EditorGUILayout.Foldout(m_importOptions, "Import Options");
				if (m_importOptions)
				{
					CsvcolumnUi(m_importColumns);
				}

				if (GUILayout.Button("Import"))
				{
					m_importPath = EditorUtility.OpenFilePanel("Browse File", m_importPath, "svg,csv");
					if (File.Exists(m_importPath))
					{
						m_splines.Clear();
						string ext = Path.GetExtension(m_importPath).ToLower();
						switch (ext)
						{
							case ".svg": ImportSvg(m_importPath); break;
							case ".csv": ImportCsv(m_importPath); break;
							case ".xml": ImportSvg(m_importPath); break;
						}
					}
				}

				m_exported = GetSelectedSplines();
				if (m_exported.Count == 0)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}

				if (m_mode == Mode.Export)
				{
					ExportUi();
				}

				if (m_mode != Mode.Export)
				{
					if (GUILayout.Button("Export") && m_exported.Count > 0)
					{
						m_mode = Mode.Export;
					}
				}
			}
		}

		private List<SplineComputer> GetSelectedSplines()
		{
			var selected = new List<SplineComputer>();
			foreach (GameObject obj in Selection.gameObjects)
			{
				var comp = obj.GetComponent<SplineComputer>();
				if (comp != null)
				{
					selected.Add(comp);
				}
			}

			return selected;
		}

		private void ApplyPoints()
		{
			if (m_originalPoints.Count != m_imported.Count)
			{
				return;
			}

			if (m_imported.Count == 0)
			{
				return;
			}

			Quaternion lookRot = Quaternion.identity;
			switch (m_importAxis)
			{
				case Axis.X: lookRot = Quaternion.LookRotation(Vector3.right); break;
				case Axis.Y: lookRot = Quaternion.LookRotation(Vector3.down); break;
				case Axis.Z: lookRot = Quaternion.LookRotation(Vector3.forward); break;
			}

			for (var i = 0; i < m_imported.Count; i++)
			{
				var transformed = new SplinePoint[m_originalPoints[i].Length];
				m_originalPoints[i].CopyTo(transformed, 0);
				for (var j = 0; j < transformed.Length; j++)
				{
					transformed[j].position *= m_scaleFactor;
					transformed[j].tangent *= m_scaleFactor;
					transformed[j].tangent2 *= m_scaleFactor;

					transformed[j].position = lookRot * transformed[j].position;
					transformed[j].tangent = lookRot * transformed[j].tangent;
					transformed[j].tangent2 = lookRot * transformed[j].tangent2;
					transformed[j].normal = lookRot * transformed[j].normal;
				}

				m_imported[i].SetPoints(transformed);
				if (m_alwaysDraw)
				{
					DssplineDrawer.RegisterComputer(m_imported[i]);
				}
				else
				{
					DssplineDrawer.UnregisterComputer(m_imported[i]);
				}
			}

			SceneView.RepaintAll();
		}

		private void GetImportedPoints()
		{
			foreach (SplineComputer comp in m_imported)
			{
				if (comp != null)
				{
					m_originalPoints.Add(comp.GetPoints(SplineComputer.Space.Local));
					m_mode = Mode.Import;
				}
				else
				{
					m_imported.Remove(comp);
				}
			}
		}

		private void ImportSvg(string file)
		{
			var svg = new Svg(file);
			m_originalPoints.Clear();
			m_imported = svg.CreateSplineComputers(Vector3.zero, Quaternion.identity);
			if (m_imported.Count == 0)
			{
				return;
			}

			m_importedParent = new GameObject(svg.name);
			foreach (SplineComputer comp in m_imported)
			{
				comp.transform.parent = m_importedParent.transform;
			}
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

			GetImportedPoints();
			ApplyPoints();
			m_promptSave = true;
		}

		private void ExportSvg(string file)
		{
			var svg = new Svg(m_exported);
			svg.Write(file, (Svg.Axis)(int)m_exportAxis);
		}

		private void ExportCsv(string file)
		{
			var csv = new Csv(m_exported[0]);
			csv.columns = m_exportColumns;
			if (m_flatCsv)
			{
				switch (m_exportAxis)
				{
					case Axis.X: csv.FlatX(); break;
					case Axis.Y: csv.FlatY(); break;
					case Axis.Z: csv.FlatZ(); break;
				}
			}

			csv.Write(file);
		}


		private void ImportCsv(string file)
		{
			var csv = new Csv(file, m_importColumns);
			m_originalPoints.Clear();
			m_imported.Clear();
			m_imported.Add(csv.CreateSplineComputer(Vector3.zero, Quaternion.identity));
			if (m_imported.Count == 0)
			{
				return;
			}
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

			GetImportedPoints();
			ApplyPoints();
			m_promptSave = true;
		}

		private enum Mode
		{
			None,
			Import,
			Export
		}

		private enum ExportFormat
		{
			SVG,
			CSV
		}

		private enum Axis
		{
			X,
			Y,
			Z
		}
	}
}
namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using System.IO;
    using Dreamteck.Splines.IO;

    public class ImportExportTool : SplineTool
    {
        private float m_scaleFactor = 1f;
        private bool m_alwaysDraw = true;
        private string m_importPath = "";
        private string m_exportPath = "";
        List<SplinePoint[]> m_originalPoints = new List<SplinePoint[]>();
        List<SplineComputer> m_imported = new List<SplineComputer>();
        List<SplineComputer> m_exported = new List<SplineComputer>();
        GameObject m_importedParent = null;

        enum Mode { None, Import, Export }
        enum ExportFormat { SVG, CSV }
        enum Axis { X, Y, Z }

        Mode m_mode = Mode.None;
        ExportFormat m_format = ExportFormat.SVG;
        Axis m_importAxis = Axis.Z;
        Axis m_exportAxis = Axis.Z;

        bool m_importOptions = false;

        List<Csv.ColumnType> m_exportColumns = new List<Csv.ColumnType>();
        List<Csv.ColumnType> m_importColumns = new List<Csv.ColumnType>();
        bool m_flatCsv = false;

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

        void LoadColumns(string name, ref List<Csv.ColumnType> destination)
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
                int i = 0;
                if (int.TryParse(element, out i)) destination.Add((Csv.ColumnType)i);
            } 
        }

        public override void Close()
        {
            base.Close(); 
            if(m_importPath != "") SaveString("importPath", Path.GetDirectoryName(m_importPath));
            if (m_exportPath != "")  SaveString("exportPath", Path.GetDirectoryName(m_exportPath));
            string columnString = ""; 
            foreach(Csv.ColumnType col in m_importColumns)
            {
                if (columnString != "") columnString += ",";
                columnString += ((int)col).ToString();
            }
            SaveString("importColumns", columnString);
            columnString = "";
            foreach (Csv.ColumnType col in m_exportColumns)
            {
                if (columnString != "") columnString += ",";
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
            } else
            {
                foreach(SplineComputer comp in m_imported)
                {
                    if(comp != null)
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
            foreach (SplineComputer spline in m_imported) GameObject.DestroyImmediate(spline.gameObject);
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

        void CsvcolumnUi(List<Csv.ColumnType> columns)
        {
            EditorGUILayout.LabelField("Dataset Columns");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.MaxWidth(30)) && columns.Count > 0) columns.RemoveAt(columns.Count - 1);
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i] = (Csv.ColumnType)EditorGUILayout.EnumPopup(columns[i]);
            }
            if (GUILayout.Button("+", GUILayout.MaxWidth(30)) && columns.Count > 0) columns.Add(Csv.ColumnType.Position);
            EditorGUILayout.EndHorizontal();
        }

        void OnScene(SceneView current)
        {
            for (int i = 0; i < m_imported.Count; i++)
            {
                DssplineDrawer.DrawSplineComputer(m_imported[i]);
            }
        }

        void ImportUi()
        {
            EditorGUI.BeginChangeCheck();
            m_scaleFactor = EditorGUILayout.FloatField("Scale Factor", m_scaleFactor);
            m_importAxis = (Axis)EditorGUILayout.EnumPopup("Facing Axis", m_importAxis);
            m_alwaysDraw = EditorGUILayout.Toggle("Always Draw", m_alwaysDraw);
            if (EditorGUI.EndChangeCheck()) ApplyPoints();
            SaveCancelUi();
        }

        void ExportUi()
        {
            if(m_exported.Count == 0)
            {
                m_mode = Mode.None;
                return;
            }
            EditorGUILayout.Space();
            m_format = (ExportFormat)EditorGUILayout.EnumPopup("Format", m_format);
            if (m_format == ExportFormat.SVG)
            {
                m_exportAxis = (Axis)EditorGUILayout.EnumPopup("Projection Axis", m_exportAxis);
                EditorGUILayout.HelpBox("The SVG is a 2D vector format so the exported spline will be flattened along the selected axis", MessageType.Info);
            }
            else
            {
                CsvcolumnUi(m_exportColumns);
                m_flatCsv = EditorGUILayout.Toggle("Flat", m_flatCsv);
                if(m_flatCsv) m_exportAxis = (Axis)EditorGUILayout.EnumPopup("Projection Axis", m_exportAxis);
                EditorGUILayout.HelpBox("The exported splined will be flattened along the selected axis.", MessageType.Info);

            }

            if (GUILayout.Button("Save File"))
            {
                string extension = "*";
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
                if (m_importOptions) CsvcolumnUi(m_importColumns);
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
                if (m_exported.Count == 0) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                if (m_mode == Mode.Export) ExportUi();
                if (m_mode != Mode.Export)
                {
                    if (GUILayout.Button("Export") && m_exported.Count > 0) m_mode = Mode.Export;
                }
            }
        }

        List<SplineComputer> GetSelectedSplines()
        {
            List<SplineComputer> selected = new List<SplineComputer>();
            foreach(GameObject obj in Selection.gameObjects)
            {
                SplineComputer comp = obj.GetComponent<SplineComputer>();
                if (comp != null) selected.Add(comp);
            }
            return selected;
        }

        void ApplyPoints()
        {
            if (m_originalPoints.Count != m_imported.Count) return;
            if (m_imported.Count == 0) return;
            Quaternion lookRot = Quaternion.identity;
            switch (m_importAxis)
            {
                case Axis.X: lookRot = Quaternion.LookRotation(Vector3.right); break;
                case Axis.Y: lookRot = Quaternion.LookRotation(Vector3.down); break;
                case Axis.Z: lookRot = Quaternion.LookRotation(Vector3.forward); break;
            }
            for (int i = 0; i < m_imported.Count; i++)
            {
                SplinePoint[] transformed = new SplinePoint[m_originalPoints[i].Length];
                m_originalPoints[i].CopyTo(transformed, 0);
                for (int j = 0; j < transformed.Length; j++)
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

        void GetImportedPoints()
        {
            foreach (SplineComputer comp in m_imported)
            {
                if (comp != null)
                {
                    m_originalPoints.Add(comp.GetPoints(SplineComputer.Space.Local));
                    m_mode = Mode.Import;
                } else m_imported.Remove(comp);
            }
        }

        void ImportSvg(string file)
        {
            Svg svg = new Svg(file);
            m_originalPoints.Clear();
            m_imported = svg.CreateSplineComputers(Vector3.zero, Quaternion.identity);
            if (m_imported.Count == 0) return;
            m_importedParent = new GameObject(svg.name);
            foreach (SplineComputer comp in m_imported) comp.transform.parent = m_importedParent.transform;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

            GetImportedPoints();
            ApplyPoints();
            m_promptSave = true;
        }

        void ExportSvg(string file)
        {
            Svg svg = new Svg(m_exported);
            svg.Write(file, (Svg.Axis)((int)m_exportAxis));
        }

        void ExportCsv(string file)
        {
            Csv csv = new Csv(m_exported[0]);
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


        void ImportCsv(string file)
        {
            Csv csv = new Csv(file, m_importColumns);
            m_originalPoints.Clear();
            m_imported.Clear();
            m_imported.Add(csv.CreateSplineComputer(Vector3.zero, Quaternion.identity));
            if (m_imported.Count == 0) return;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

            GetImportedPoints();
            ApplyPoints();
            m_promptSave = true;
        }
    }
}

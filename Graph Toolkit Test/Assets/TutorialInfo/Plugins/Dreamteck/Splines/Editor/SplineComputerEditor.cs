namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEditor;

    [CustomEditor(typeof(SplineComputer), true)]
    [CanEditMultipleObjects]
    public partial class SplineComputerEditor : Editor 
    {
        public SplineComputer spline;
        public SplineComputer[] splines = new SplineComputer[0];
        public static bool hold = false;

        public int[] pointSelection
        {
            get
            {
                return m_selectedPoints.ToArray();
            }
        }

        public int selectedPointsCount
        {
            get { return m_selectedPoints.Count; }
            set { }
        }

        protected bool m_closedOnMirror = false;

        private DreamteckSplinesEditor m_pathEditor;
        private ComputerEditor m_computerEditor;
        private SplineTriggersEditor m_triggersEditor;
        private SplineComputerDebugEditor m_debugEditor;
        private bool m_rebuildSpline = false;
        private List<int> m_selectedPoints = new List<int>();


        [MenuItem("GameObject/3D Object/Spline Computer")]
        private static void NewEmptySpline()
        {
            int count = GameObject.FindObjectsOfType<SplineComputer>().Length;
            string objName = "Spline";
            if (count > 0) objName += " " + count;
            GameObject obj = new GameObject(objName);
            obj.AddComponent<SplineComputer>();
            if (Selection.activeGameObject != null)
            {
                if (EditorUtility.DisplayDialog("Make child?", "Do you want to make the new spline a child of " + Selection.activeGameObject.name + "?", "Yes", "No"))
                {
                    obj.transform.parent = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }
            }
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/3D Object/Spline Node")]
        private static void NewSplineNode()
        {
            int count = Object.FindObjectsOfType<Node>().Length;
            string objName = "Node";
            if (count > 0) objName += " " + count;
            GameObject obj = new GameObject(objName);
            obj.AddComponent<Node>();
            if(Selection.activeGameObject != null)
            {
                if(EditorUtility.DisplayDialog("Make child?", "Do you want to make the new node a child of " + Selection.activeGameObject.name + "?", "Yes", "No"))
                {
                    obj.transform.parent = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }
            }
            Selection.activeGameObject = obj;
        }

        public void UndoRedoPerformed()
        {
            m_pathEditor.UndoRedoPerformed();
            spline.EditorUpdateConnectedNodes();
            spline.Rebuild();
        }

        private void OnEnable()
        {
            splines = new SplineComputer[targets.Length];
            for (int i = 0; i < splines.Length; i++)
            {
                splines[i] = (SplineComputer)targets[i];
                splines[i].EditorAwake();
                if (splines[i].editorAlwaysDraw)
                {
                    DssplineDrawer.RegisterComputer(splines[i]);
                }
            }
            spline = splines[0];
            InitializeSplineEditor();
            InitializeComputerEditor();
            m_debugEditor = new SplineComputerDebugEditor(spline, serializedObject, m_pathEditor);
            m_debugEditor.undoHandler += RecordUndo;
            m_debugEditor.repaintHandler += OnRepaint;
            m_triggersEditor = new SplineTriggersEditor(spline, serializedObject);
            m_triggersEditor.undoHandler += RecordUndo;
            m_triggersEditor.repaintHandler += OnRepaint;
            hold = false;
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui += BeforeSceneGui;
            SceneView.duringSceneGui += DuringSceneGui;
#else
            SceneView.onSceneGUIDelegate += BeforeSceneGUI;
            SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void BeforeSceneGui(SceneView current)
        {
            m_pathEditor.BeforeSceneGui(current);

            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    for (int i = 0; i < splines.Length; i++)
                    {
                        if (splines[i].editorUpdateMode == SplineComputer.EditorUpdateMode.OnMouseUp)
                        {
                            splines[i].RebuildImmediate();
                        }
                    }
                }
            }
        }

        private void InitializeSplineEditor()
        {
            m_pathEditor = new DreamteckSplinesEditor(spline, serializedObject);
            m_pathEditor.undoHandler = RecordUndo;
            m_pathEditor.repaintHandler = OnRepaint;
            m_pathEditor.editSpace = (SplineEditor.Space)SplinePrefs.pointEditSpace;
        }

        private void InitializeComputerEditor()
        {
            m_computerEditor = new ComputerEditor(splines, serializedObject, m_pathEditor);
            m_computerEditor.undoHandler = RecordUndo;
            m_computerEditor.repaintHandler = OnRepaint;
        }

        private void RecordUndo(string title)
        {
            for (int i = 0; i < splines.Length; i++)
            {
                Undo.RecordObject(splines[i], title);
            }
        }

        private void OnRepaint()
        {
            SceneView.RepaintAll();
            Repaint();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui -= BeforeSceneGui;
            SceneView.duringSceneGui -= DuringSceneGui;
#else
            SceneView.onSceneGUIDelegate -= BeforeSceneGUI;
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
            m_pathEditor.Destroy();
            m_computerEditor.Destroy();
            m_debugEditor.Destroy();
            m_triggersEditor.Destroy();
        }

        public override void OnInspectorGUI()
        {
            if (m_debugEditor.editorUpdateMode == SplineComputer.EditorUpdateMode.OnMouseUp)
            {
                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    m_rebuildSpline = true;
                }
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
                {
                    m_rebuildSpline = true;
                }
            }
            base.OnInspectorGUI();
            spline = (SplineComputer)target;

            if (splines.Length == 1)
            {
                SplineEditorGui.BeginContainerBox(ref m_pathEditor.open, "Edit");
                if (m_pathEditor.open)
                {
                    SplineEditor.Space lastSpace = m_pathEditor.editSpace;
                    m_pathEditor.DrawInspector();
                    if (lastSpace != m_pathEditor.editSpace)
                    {
                        SplinePrefs.pointEditSpace = (SplineComputer.Space)m_pathEditor.editSpace;
                        SplinePrefs.SavePrefs();
                    }
                }
                else if (m_pathEditor.lastEditorTool != Tool.None && Tools.current == Tool.None)
                {
                    Tools.current = m_pathEditor.lastEditorTool;
                }
                SplineEditorGui.EndContainerBox();
            }

            SplineEditorGui.BeginContainerBox(ref m_computerEditor.open, "Spline Computer");
            if (m_computerEditor.open)
            {
                m_computerEditor.DrawInspector();
            }
            SplineEditorGui.EndContainerBox();

            if (splines.Length == 1)
            {
                SplineEditorGui.BeginContainerBox(ref m_triggersEditor.open, "Triggers");
                if (m_triggersEditor.open) m_triggersEditor.DrawInspector();
                SplineEditorGui.EndContainerBox();
            }

            SplineEditorGui.BeginContainerBox(ref m_debugEditor.open, "Editor Properties");
            if (m_debugEditor.open) m_debugEditor.DrawInspector();
            SplineEditorGui.EndContainerBox();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(spline);
            }


            if (Event.current.type == EventType.Layout && m_rebuildSpline)
            {
                for (int i = 0; i < splines.Length; i++)
                {
                    if (splines[i].editorUpdateMode == SplineComputer.EditorUpdateMode.OnMouseUp)
                    {
                        splines[i].RebuildImmediate(true);
                    }
                }
                m_rebuildSpline = false;
            }

        }

        public bool IsPointSelected(int index)
        {
            return m_selectedPoints.Contains(index);
        }

        private void DuringSceneGui(SceneView currentSceneView)
        {
            m_debugEditor.DrawScene(currentSceneView);
            m_computerEditor.drawComputer = !(m_pathEditor.currentModule is CreatePointModule);
            m_computerEditor.drawPivot = m_pathEditor.open && spline.editorDrawPivot;
            m_computerEditor.DrawScene(currentSceneView);
            if (splines.Length == 1 && m_triggersEditor.open) m_triggersEditor.DrawScene(currentSceneView);
            if (splines.Length == 1 && m_pathEditor.open) m_pathEditor.DrawScene(currentSceneView);
        }
    }
}
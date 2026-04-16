namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class ComputerEditor : SplineEditorBase
    {
        public bool drawComputer = true;
        public bool drawPivot = true;
        public bool drawConnectedComputers = true;
        private DreamteckSplinesEditor m_pathEditor;
        private int m_operation = -1, m_module = -1, m_transformTool = 1;
        private ComputerEditorModule[] m_modules = new ComputerEditorModule[0];
        private Dreamteck.Editor.Toolbar m_utilityToolbar;
        private Dreamteck.Editor.Toolbar m_operationsToolbar;
        private Dreamteck.Editor.Toolbar m_transformToolbar;
        private SplineComputer m_spline = null;
        private SplineComputer[] m_splines = new SplineComputer[0];
        private bool m_pathToolsFoldout = false, m_interpolationFoldout = false;

        private SerializedProperty m_splineProperty;
        private SerializedProperty m_sampleRate;
        private SerializedProperty m_type;
        private SerializedProperty m_knotParametrization;
        private SerializedProperty m_linearAverageDirection;
        private SerializedProperty m_space;
        private SerializedProperty m_sampleMode;
        private SerializedProperty m_optimizeAngleThreshold;
        private SerializedProperty m_updateMode;
        private SerializedProperty m_multithreaded;
        private SerializedProperty m_customNormalInterpolation;
        private SerializedProperty m_customValueInterpolation;


        public ComputerEditor(SplineComputer[] splines, SerializedObject serializedObject, DreamteckSplinesEditor pathEditor) : base(serializedObject)
        {
            m_spline = splines[0];
            this.m_splines = splines;
            this.m_pathEditor = pathEditor;

            m_splineProperty = serializedObject.FindProperty("_spline");
            m_sampleRate = serializedObject.FindProperty("_spline").FindPropertyRelative("sampleRate");
            m_type = serializedObject.FindProperty("_spline").FindPropertyRelative("type");
            m_linearAverageDirection = m_splineProperty.FindPropertyRelative("linearAverageDirection");
            m_space = serializedObject.FindProperty("_space");
            m_sampleMode = serializedObject.FindProperty("_sampleMode");
            m_optimizeAngleThreshold = serializedObject.FindProperty("_optimizeAngleThreshold");
            m_updateMode = serializedObject.FindProperty("updateMode");
            m_multithreaded = serializedObject.FindProperty("multithreaded");
            m_customNormalInterpolation = m_splineProperty.FindPropertyRelative("customNormalInterpolation");
            m_customValueInterpolation = m_splineProperty.FindPropertyRelative("customValueInterpolation");
            m_knotParametrization = m_splineProperty.FindPropertyRelative("_knotParametrization");


            m_modules = new ComputerEditorModule[2];
            m_modules[0] = new ComputerMergeModule(m_spline);
            m_modules[1] = new ComputerSplitModule(m_spline);
            GUIContent[] utilityContents = new GUIContent[m_modules.Length], utilityContentsSelected = new GUIContent[m_modules.Length];
            for (int i = 0; i < m_modules.Length; i++)
            {
                utilityContents[i] = m_modules[i].GetIconOff();
                utilityContentsSelected[i] = m_modules[i].GetIconOn();
                m_modules[i].undoHandler += OnRecordUndo;
                m_modules[i].repaintHandler += OnRepaint;
            }
            m_utilityToolbar = new Dreamteck.Editor.Toolbar(utilityContents, utilityContentsSelected, 35f);
            m_utilityToolbar.newLine = false;


            int index = 0;
            GUIContent[] transformContents = new GUIContent[4], transformContentsSelected = new GUIContent[4];
            transformContents[index] = new GUIContent("OFF");
            transformContentsSelected[index++] = new GUIContent("OFF");

            transformContents[index] = EditorGUIUtility.IconContent("MoveTool");
            transformContentsSelected[index++] = EditorGUIUtility.IconContent("MoveTool On");

            transformContents[index] = EditorGUIUtility.IconContent("RotateTool");
            transformContentsSelected[index++] = EditorGUIUtility.IconContent("RotateTool On");

            transformContents[index] = EditorGUIUtility.IconContent("ScaleTool");
            transformContentsSelected[index] = EditorGUIUtility.IconContent("ScaleTool On");

            m_transformToolbar = new Dreamteck.Editor.Toolbar(transformContents, transformContentsSelected, 35f);
            m_transformToolbar.newLine = false;

            index = 0;
            GUIContent[] operationContents = new GUIContent[3], operationContentsSelected = new GUIContent[3];
            for (int i = 0; i < operationContents.Length; i++)
            {
                operationContents[i] = new GUIContent("");
                operationContentsSelected[i] = new GUIContent("");
            }
            m_operationsToolbar = new Dreamteck.Editor.Toolbar(operationContents, operationContentsSelected, 64f);
            m_operationsToolbar.newLine = false;
        }

        void OnRecordUndo(string title)
        {
            if (undoHandler != null) undoHandler(title);
        }

        void OnRepaint()
        {
            if (repaintHandler != null) repaintHandler();
        }

        protected override void Load()
        {
            base.Load();
            m_pathToolsFoldout = LoadBool("DreamteckSplinesEditor.pathToolsFoldout", false);
            m_interpolationFoldout = LoadBool("DreamteckSplinesEditor.interpolationFoldout", false);
            m_transformTool = LoadInt("DreamteckSplinesEditor.transformTool", 0);
        }

        protected override void Save()
        {
            base.Save();
            SaveBool("DreamteckSplinesEditor.pathToolsFoldout", m_pathToolsFoldout);
            SaveBool("DreamteckSplinesEditor.interpolationFoldout", m_interpolationFoldout);
            SaveInt("DreamteckSplinesEditor.transformTool", m_transformTool);
        }

        public override void Destroy()
        {
            base.Destroy();
            for (int i = 0; i < m_modules.Length; i++) m_modules[i].Deselect();
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (m_spline == null) return;
            SplineEditorGui.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            m_operationsToolbar.SetContent(0, new GUIContent(m_spline.isClosed ? "Break" : "Close"));
            m_operationsToolbar.SetContent(1, new GUIContent("Reverse"));
            m_operationsToolbar.SetContent(2, new GUIContent(m_spline.is2D ? "3D Mode" : "2D Mode"));
            m_operationsToolbar.Draw(ref m_operation);
            if (EditorGUI.EndChangeCheck())
            {
                PerformOperation();
            }
            EditorGUI.BeginChangeCheck();
            if (m_splines.Length == 1)
            {
                int mod = m_module;
                m_utilityToolbar.Draw(ref mod);
                if (EditorGUI.EndChangeCheck())
                {
                    ToggleModule(mod);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (m_module >= 0 && m_module < m_modules.Length)
            {
                m_modules[m_module].DrawInspector();
            }
            EditorGUILayout.Space();
            DreamteckEditorGui.DrawSeparator();

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            Spline.Type lastType = (Spline.Type)m_type.intValue;
            EditorGUILayout.PropertyField(m_type);
            if(lastType == Spline.Type.CatmullRom && m_type.intValue == (int)Spline.Type.Bezier)
            {
                if(EditorUtility.DisplayDialog("Hermite to Bezier", "Would you like to retain the Catmull Rom shape in Bezier mode?", "Yes", "No"))
                {
                    for (int i = 0; i < m_splines.Length; i++)
                    {
                        Undo.RecordObject(m_splines[i], "CatToBezierTangents");
                        m_splines[i].CatToBezierTangents();
                        EditorUtility.SetDirty(m_splines[i]);
                    }
                    m_pathEditor.SetPointsArray(m_spline.GetPoints());
                    m_pathEditor.ApplyModifiedProperties();
                }
            }

            if(m_spline.type == Spline.Type.CatmullRom)
            {
                int type = Mathf.RoundToInt(m_knotParametrization.floatValue * 2);
                string catmullTypeText = "Parametrization: ";
                switch (type)
                {
                    case 0: catmullTypeText += "Uniform"; break;
                    case 1: catmullTypeText += "Centripetal"; break;
                    case 2: catmullTypeText += "Chordal"; break;
                }
                EditorGUILayout.PropertyField(m_knotParametrization, new GUIContent(catmullTypeText));
            }

            if (m_spline.type == Spline.Type.Linear)
            {
                EditorGUILayout.PropertyField(m_linearAverageDirection);
            }

            int lastSpace = m_space.intValue;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_space, new GUIContent("Space"));
            if((SplineComputer.Space)m_space.enumValueIndex == SplineComputer.Space.Local)
            {
                EditTransformToolbar();
                if (m_splines.Length == 1)
                {
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.PropertyField(m_sampleMode, new GUIContent("Sample Mode"));
            if (m_sampleMode.intValue == (int)SplineComputer.SampleMode.Optimized) EditorGUILayout.PropertyField(m_optimizeAngleThreshold);
            EditorGUILayout.PropertyField(m_updateMode);
            if (m_updateMode.intValue == (int)SplineComputer.UpdateMode.None && Application.isPlaying)
            {
                if (GUILayout.Button("Rebuild"))
                {
                    for (int i = 0; i < m_splines.Length; i++) m_splines[i].RebuildImmediate(true, true);
                }
            }
            if (m_spline.type != Spline.Type.Linear) EditorGUILayout.PropertyField(m_sampleRate, new GUIContent("Sample Rate"));
            EditorGUILayout.PropertyField(m_multithreaded);

            EditorGUI.indentLevel++;
            bool curveUpdate = false;
            m_interpolationFoldout = EditorGUILayout.Foldout(m_interpolationFoldout, "Point Value Interpolation");
            if (m_interpolationFoldout)
            {
                if (m_customValueInterpolation.animationCurveValue == null || m_customValueInterpolation.animationCurveValue.keys.Length == 0)
                {
                    if (GUILayout.Button("Size & Color Interpolation"))
                    {
                        AnimationCurve curve = new AnimationCurve();
                        curve.AddKey(new Keyframe(0, 0, 0, 0));
                        curve.AddKey(new Keyframe(1, 1, 0, 0));
                        for (int i = 0; i < m_splines.Length; i++) m_splines[i].customValueInterpolation = curve;
                        m_serializedObject.Update();
                        m_pathEditor.GetPointsFromSpline();
                        curveUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_customValueInterpolation, new GUIContent("Size & Color Interpolation"));
                    if (GUILayout.Button("x", GUILayout.MaxWidth(25)))
                    {
                        m_customValueInterpolation.animationCurveValue = null;
                        for (int i = 0; i < m_splines.Length; i++) m_splines[i].customValueInterpolation = null;
                        m_serializedObject.Update();
                        m_pathEditor.GetPointsFromSpline();
                        curveUpdate = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (m_customNormalInterpolation.animationCurveValue == null || m_customNormalInterpolation.animationCurveValue.keys.Length == 0)
                {
                    if (GUILayout.Button("Normal Interpolation"))
                    {
                        AnimationCurve curve = new AnimationCurve();
                        curve.AddKey(new Keyframe(0, 0));
                        curve.AddKey(new Keyframe(1, 1));
                        for (int i = 0; i < m_splines.Length; i++) m_splines[i].customNormalInterpolation = curve;
                        m_serializedObject.Update();
                        m_pathEditor.GetPointsFromSpline();
                        curveUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_customNormalInterpolation, new GUIContent("Normal Interpolation"));
                    if (GUILayout.Button("x", GUILayout.MaxWidth(25)))
                    {
                        m_customNormalInterpolation.animationCurveValue = null;
                        for (int i = 0; i < m_splines.Length; i++) m_splines[i].customNormalInterpolation = null;
                        m_serializedObject.Update();
                        m_pathEditor.GetPointsFromSpline();
                        curveUpdate = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck() || curveUpdate)
            {
                if (m_sampleRate.intValue < 2)
                {
                    m_sampleRate.intValue = 2;
                }

                bool forceUpdateAll = false;
                if (lastSpace != m_space.intValue)
                {
                    forceUpdateAll = true;
                }

                m_pathEditor.ApplyModifiedProperties(true);

                for (int i = 1; i < m_splines.Length; i++)
                {
                    m_splines[i].RebuildImmediate(true, forceUpdateAll);
                }
            }

            if (m_pathEditor.currentModule != null)
            {
                m_transformTool = 0;
            }
        }

        private void EditTransformToolbar()
        {
            if(m_splines.Length > 1)
            {
                GUILayout.Label("Edit Transform unavailable with multiple splines");
                return;
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Edit Transform - Only");
            GUILayout.FlexibleSpace();
            int lastTool = m_transformTool;
            m_transformToolbar.Draw(ref m_transformTool);
            if (lastTool != m_transformTool && m_transformTool > 0)
            {
                m_pathEditor.UntoggleCurrentModule();
                Tools.current = Tool.None;
            }
            EditorGUILayout.EndHorizontal();

            switch (m_transformTool)
            {
                case 1:
                    Vector3 position = m_spline.transform.position;

                    position = EditorGUILayout.Vector3Field("Position", position);
                    if (position != m_spline.transform.position)
                    {
                        Undo.RecordObject(m_spline.transform, "Move spline computer");
                        m_spline.transform.position = position;
                        m_pathEditor.ApplyModifiedProperties(true);
                    }
                    break;
                case 2:
                    Quaternion rotation = m_spline.transform.rotation;
                    rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", rotation.eulerAngles));
                    if (rotation != m_spline.transform.rotation)
                    {
                        Undo.RecordObject(m_spline.transform, "Rotate spline computer");
                        m_spline.transform.rotation = rotation;
                        m_pathEditor.ApplyModifiedProperties(true);
                    }
                    break;
                case 3:
                    Vector3 scale = m_spline.transform.localScale;
                    scale = EditorGUILayout.Vector3Field("Scale", scale);
                    if (scale != m_spline.transform.localScale)
                    {
                        Undo.RecordObject(m_spline.transform, "Scale spline computer");
                        m_spline.transform.localScale = scale;
                        m_pathEditor.ApplyModifiedProperties(true);
                    }
                    break;
            }
        }

        void PerformOperation()
        {
            switch (m_operation)
            {
                case 0:
                    if (m_spline.isClosed)
                    {
                        BreakSpline();
                    }
                    else
                    {
                        CloseSpline();
                    }
                    m_operation = -1;
                    break;
                case 1:
                    ReversePointOrder();
                    m_operation = -1;
                    break;
                case 2:
                    {
                        m_pathEditor.is2D = !m_pathEditor.is2D;

                        if (m_pathEditor.is2D)
                        {
                            for (int i = 0; i < m_pathEditor.points.Length; i++)
                            {
                                FlattenPoint(ref m_pathEditor.points[i], LinearAlgebraUtility.Axis.Z);
                            }
                        }

                        m_pathEditor.ApplyModifiedProperties();
                        m_operation = -1;
                    }
                    break;
            }
        }

        private void FlattenPoint(ref SerializedSplinePoint point, LinearAlgebraUtility.Axis axis, float flatValue = 0f)
        {
            point.position = LinearAlgebraUtility.FlattenVector(point.position, axis, flatValue);
            point.tangent = LinearAlgebraUtility.FlattenVector(point.tangent, axis, flatValue);
            point.tangent2 = LinearAlgebraUtility.FlattenVector(point.tangent2, axis, flatValue);
            switch (axis)
            {
                case LinearAlgebraUtility.Axis.X: point.normal = Vector3.right; break;
                case LinearAlgebraUtility.Axis.Y: point.normal = Vector3.up; break;
                case LinearAlgebraUtility.Axis.Z: point.normal = Vector3.forward; break;
            }
        }

        void ToggleModule(int index)
        {
            if (m_module >= 0 && m_module < m_modules.Length) m_modules[m_module].Deselect();
            if (m_module == index) index = -1;
            m_module = index;
            if (m_module >= 0 && m_module < m_modules.Length) m_modules[m_module].Select();
        }

        public void BreakSpline()
        {
            RecordUndo("Break path");
            if (m_splines.Length == 1 && m_pathEditor.selectedPoints.Count == 1)
            {
                m_spline.Break(m_pathEditor.selectedPoints[0]);
                EditorUtility.SetDirty(m_spline);
                m_pathEditor.selectedPoints.Clear();
                m_pathEditor.selectedPoints.Add(0);

            }
            else
            {
                for (int i = 0; i < m_splines.Length; i++)
                {
                    m_splines[i].Break();
                    EditorUtility.SetDirty(m_splines[i]);
                }
            }
        }

        public void CloseSpline()
        {
            RecordUndo("Close path");
            for (int i = 0; i < m_splines.Length; i++)
            {
                m_splines[i].Close();
            }
        }

        void ReversePointOrder()
        {
            for (int i = 0; i < m_splines.Length; i++)
            {
                ReversePointOrder(m_splines[i]);
            }
        }

        void ReversePointOrder(SplineComputer spline)
        {
            SplinePoint[] points = spline.GetPoints();
            for (int i = 0; i < Mathf.FloorToInt(points.Length / 2); i++)
            {
                SplinePoint temp = points[i];
                points[i] = points[(points.Length - 1) - i];
                Vector3 tempTan = points[i].tangent;
                points[i].tangent = points[i].tangent2;
                points[i].tangent2 = tempTan;
                int opposideIndex = (points.Length - 1) - i;
                points[opposideIndex] = temp;
                tempTan = points[opposideIndex].tangent;
                points[opposideIndex].tangent = points[opposideIndex].tangent2;
                points[opposideIndex].tangent2 = tempTan;
            }
            if (points.Length % 2 != 0)
            {
                Vector3 tempTan = points[Mathf.CeilToInt(points.Length / 2)].tangent;
                points[Mathf.CeilToInt(points.Length / 2)].tangent = points[Mathf.CeilToInt(points.Length / 2)].tangent2;
                points[Mathf.CeilToInt(points.Length / 2)].tangent2 = tempTan;
            }
            spline.SetPoints(points);
        }

        public override void DrawScene(SceneView current)
        {
            base.DrawScene(current);

            if (m_spline == null)
            {
                return;
            }

            for (int i = 0; i < m_splines.Length; i++)
            {
                if (drawComputer)
                {
                    DssplineDrawer.DrawSplineComputer(m_splines[i]);
                }

                if (drawPivot)
                {
                    var trs = m_splines[i].transform;
                    float size = HandleUtility.GetHandleSize(trs.position);
                    Handles.color = Color.red;
                    Handles.DrawLine(trs.position, trs.position + trs.right * size * 0.5f);
                    Handles.color = Color.green;
                    Handles.DrawLine(trs.position, trs.position + trs.up * size * 0.5f);
                    Handles.color = Color.blue;
                    Handles.DrawLine(trs.position, trs.position + trs.forward * size * 0.5f);
                }
            }

            if (drawConnectedComputers)
            {
                for (int i = 0; i < m_splines.Length; i++)
                {
                    List<SplineComputer> computers = m_splines[i].GetConnectedComputers();
                    for (int j = 1; j < computers.Count; j++)
                    {
                        DssplineDrawer.DrawSplineComputer(computers[j], 0.0, 1.0, 0.5f);
                    }
                }
            }


            if (m_pathEditor.currentModule == null)
            {
                if(m_splines.Length > 1 || Tools.current != Tool.None)
                {
                    m_transformTool = 0;
                }
                switch (m_transformTool)
                {
                    case 1:
                        Vector3 position = m_spline.transform.position;
                        position = Handles.PositionHandle(position, m_spline.transform.rotation);
                        if (position != m_spline.transform.position)
                        {
                            Undo.RecordObject(m_spline.transform, "Move spline computer");
                            m_spline.transform.position = position;
                            m_pathEditor.ApplyModifiedProperties(true);
                        }
                        break;
                    case 2:
                        Quaternion rotation = m_spline.transform.rotation;
                        rotation = Handles.RotationHandle(rotation, m_spline.transform.position);
                        if (rotation != m_spline.transform.rotation)
                        {
                            Undo.RecordObject(m_spline.transform, "Rotate spline computer");
                            m_spline.transform.rotation = rotation;
                            m_pathEditor.ApplyModifiedProperties(true);
                        }
                        break;
                    case 3:
                        Vector3 scale = m_spline.transform.localScale;
                        scale = Handles.ScaleHandle(scale, m_spline.transform.position, m_spline.transform.rotation,
                            HandleUtility.GetHandleSize(m_spline.transform.position));
                        if (scale != m_spline.transform.localScale)
                        {
                            Undo.RecordObject(m_spline.transform, "Scale spline computer");
                            m_spline.transform.localScale = scale;
                            m_pathEditor.ApplyModifiedProperties(true);
                        }
                        break;
                }
                if (m_transformTool > 0)
                {
                    Vector2 screenPosition = HandleUtility.WorldToGUIPoint(m_spline.transform.position);
                    screenPosition.y += 20f;
                    Handles.BeginGUI();
                    DreamteckEditorGui.Label(new Rect(screenPosition.x - 120 + m_spline.name.Length * 4, screenPosition.y, 120, 25), m_spline.name);
                    Handles.EndGUI();
                }
            }
            if (m_module >= 0 && m_module < m_modules.Length) m_modules[m_module].DrawScene();
        }
    }
}

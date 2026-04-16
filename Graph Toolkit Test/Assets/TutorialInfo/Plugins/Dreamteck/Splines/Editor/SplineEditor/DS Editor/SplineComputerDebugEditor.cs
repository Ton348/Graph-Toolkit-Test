namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class SplineComputerDebugEditor : SplineEditorBase
    {
        public SplineComputer.EditorUpdateMode editorUpdateMode
        {
            get
            {
                return (SplineComputer.EditorUpdateMode)m_editorUpdateMode.enumValueIndex;
            }
        }

        private SerializedProperty m_editorDrawPivot;
        private SerializedProperty m_editorPathColor;
        private SerializedProperty m_editorAlwaysDraw;
        private SerializedProperty m_editorDrawThickness;
        private SerializedProperty m_editorBillboardThickness;
        private SerializedProperty m_editorUpdateMode;
        private SplineComputer m_spline;
        private DreamteckSplinesEditor m_pathEditor;
        private float m_length = 0f;

        public SplineComputerDebugEditor(SplineComputer spline, SerializedObject serializedObject, DreamteckSplinesEditor pathEditor) : base(serializedObject)
        {
            m_spline = spline;
            m_pathEditor = pathEditor;
            GetSplineLength();
            m_editorPathColor = serializedObject.FindProperty("editorPathColor");
            m_editorAlwaysDraw = serializedObject.FindProperty("editorAlwaysDraw");
            m_editorDrawThickness = serializedObject.FindProperty("editorDrawThickness");
            m_editorBillboardThickness = serializedObject.FindProperty("editorBillboardThickness");
            m_editorUpdateMode = serializedObject.FindProperty("editorUpdateMode");
            m_editorDrawPivot = serializedObject.FindProperty("editorDrawPivot");
        }

        void GetSplineLength()
        {
            m_length = Mathf.RoundToInt(m_spline.CalculateLength() * 100f) / 100f;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (Event.current.type == EventType.MouseUp) GetSplineLength();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_editorUpdateMode, new GUIContent("Editor Update Mode"));
            EditorGUILayout.PropertyField(m_editorPathColor, new GUIContent("Color in Scene"));
            bool lastAlwaysDraw = m_editorAlwaysDraw.boolValue;
            EditorGUILayout.PropertyField(m_editorDrawPivot, new GUIContent("Draw Transform Pivot"));
            EditorGUILayout.PropertyField(m_editorAlwaysDraw, new GUIContent("Always Draw Spline"));
            if (lastAlwaysDraw != m_editorAlwaysDraw.boolValue)
            {
                if (m_editorAlwaysDraw.boolValue)
                {
                    for (int i = 0; i < m_serializedObject.targetObjects.Length; i++)
                    {
                        if (m_serializedObject.targetObjects[i] is SplineComputer)
                        {
                            DssplineDrawer.RegisterComputer((SplineComputer)m_serializedObject.targetObjects[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_serializedObject.targetObjects.Length; i++)
                    {
                        if (m_serializedObject.targetObjects[i] is SplineComputer)
                        {
                            DssplineDrawer.UnregisterComputer((SplineComputer)m_serializedObject.targetObjects[i]);
                        }
                    }
                }
            }
            EditorGUILayout.PropertyField(m_editorDrawThickness, new GUIContent("Draw thickness"));
            if (m_editorDrawThickness.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_editorBillboardThickness, new GUIContent("Always face camera"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            if (m_serializedObject.targetObjects.Length == 1)
            {
                EditorGUILayout.HelpBox("Samples: " + m_spline.sampleCount + "\n\r" + "Length: " + m_length, MessageType.Info);
            } else
            {
                EditorGUILayout.HelpBox("Multiple spline objects selected" + m_length, MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (editorUpdateMode == SplineComputer.EditorUpdateMode.Default)
                {
                    for (int i = 0; i < m_serializedObject.targetObjects.Length; i++)
                    {
                        if(m_serializedObject.targetObjects[i] is SplineComputer)
                        {
                            ((SplineComputer)m_serializedObject.targetObjects[i]).RebuildImmediate(true);
                        }
                    }
                    SceneView.RepaintAll();
                }
                m_pathEditor.ApplyModifiedProperties();
            }
        }

        public override void DrawScene(SceneView current)
        {
            base.DrawScene(current);
            if (Event.current.type == EventType.MouseUp && open)
            {
                GetSplineLength();
            }
        }
    }
}

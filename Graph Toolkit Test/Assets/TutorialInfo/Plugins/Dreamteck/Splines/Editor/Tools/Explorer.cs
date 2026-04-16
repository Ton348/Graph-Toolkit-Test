
namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class Explorer : SplineTool
    {
        GUIStyle m_normalRow;
        GUIStyle m_selectedRow;
        List<SplineComputer> m_sceneSplines = new List<SplineComputer>();
        List<int> m_selected = new List<int>();
        Vector2 m_scroll = Vector2.zero;
        bool m_mouseLeft = false;

        public override string GetName()
        {
            return "Spline Explorer";
        }

        protected override string GetPrefix()
        {
            return "SplineExplorer";
        }

        public override void Open(EditorWindow window)
        {
            base.Open(window);
            m_normalRow = new GUIStyle(GUI.skin.box);
            m_normalRow.normal.background = null;
            m_normalRow.alignment = TextAnchor.MiddleLeft;
            m_selectedRow = new GUIStyle(m_normalRow);
            m_selectedRow.normal.background = SplineEditorGui.white;
            m_selectedRow.normal.textColor = SplinePrefs.highlightContentColor;
            GetSceneSplines();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnScene;
#else
            SceneView.onSceneGUIDelegate += OnScene;
#endif

        }

        public override void Close()
        {
            base.Close();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

        }

        void OnScene(SceneView current)
        {
            if(m_selected.Count > 1)
            {
                for (int i = 0; i < m_selected.Count; i++)
                {
                    if (!m_sceneSplines[m_selected[i]].editorAlwaysDraw)
                    {
                        DssplineDrawer.DrawSplineComputer(m_sceneSplines[m_selected[i]]);
                    }
                }
            }
        }

        void GetSceneSplines()
        {
            m_sceneSplines = new List<SplineComputer>(Resources.FindObjectsOfTypeAll<SplineComputer>());
        }

        public override void Draw(Rect rect)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0) m_mouseLeft = true; 
                    break;
                case EventType.MouseUp: if (Event.current.button == 0) m_mouseLeft = false; break;
            }

            Rect lastRect;
            m_scroll = EditorGUILayout.BeginScrollView(m_scroll, GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            EditorGUILayout.BeginHorizontal(m_normalRow);
            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel, GUILayout.Width(rect.width - 200));
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel, GUILayout.Width(65));
            EditorGUILayout.LabelField("Draw", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.LabelField("Thickness", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < m_sceneSplines.Count; i++)
            {
                bool isSelected = m_selected.Contains(i);
                if (isSelected) GUI.backgroundColor = SplinePrefs.highlightColor;
                
                EditorGUILayout.BeginHorizontal(isSelected ? m_selectedRow : m_normalRow);
                EditorGUILayout.LabelField(m_sceneSplines[i].name, isSelected ? m_selectedRow : m_normalRow, GUILayout.Width(rect.width-200));
                GUI.backgroundColor = Color.white;
                Color pathColor = m_sceneSplines[i].editorPathColor;
                pathColor = EditorGUILayout.ColorField(pathColor, GUILayout.Width(65));
                if(pathColor != m_sceneSplines[i].editorPathColor)
                {
                    foreach (int index in m_selected) m_sceneSplines[index].editorPathColor = pathColor;
                }
                bool alwaysDraw = m_sceneSplines[i].editorAlwaysDraw;
                alwaysDraw = EditorGUILayout.Toggle(alwaysDraw, GUILayout.Width(40));
                if(alwaysDraw != m_sceneSplines[i].editorAlwaysDraw)
                {
                    foreach (int index in m_selected)
                    {
                        if (alwaysDraw)
                        {
                            DssplineDrawer.RegisterComputer(m_sceneSplines[index]);
                        }
                        else
                        {
                            DssplineDrawer.UnregisterComputer(m_sceneSplines[index]);
                        }
                    }
                }
                bool thickness = m_sceneSplines[i].editorDrawThickness;
                thickness = EditorGUILayout.Toggle(thickness, GUILayout.Width(40));
                if(thickness != m_sceneSplines[i].editorDrawThickness)
                {
                    foreach (int index in m_selected) m_sceneSplines[index].editorDrawThickness = thickness;
                }
                EditorGUILayout.EndHorizontal();
                lastRect = GUILayoutUtility.GetLastRect();
                if (m_mouseLeft)
                {
                    if (lastRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.control)
                        {
                            if (!m_selected.Contains(i)) m_selected.Add(i);
                        }
                        else if (m_selected.Count > 0 && Event.current.shift)
                        {
                            int closest = m_selected[0];
                            int delta = m_sceneSplines.Count;
                            for (int j = 0; j < m_selected.Count; j++)
                            {
                                int d = Mathf.Abs(i - m_selected[j]);
                                if (d < delta)
                                {
                                    delta = d;
                                    closest = m_selected[j];
                                }
                            }
                            if (closest < i)
                            {
                                for (int j = closest + 1; j <= i; j++)
                                {
                                    if (m_selected.Contains(j)) continue;
                                    m_selected.Add(j);
                                }
                            }
                            else
                            {
                                for (int j = closest - 1; j >= i; j--)
                                {
                                    if (m_selected.Contains(j)) continue;
                                    m_selected.Add(j);
                                }
                            }
                        }
                        else m_selected = new List<int>(new int[] { i });
                        List<GameObject> selectGo = new List<GameObject>();
                        foreach(int index in m_selected) selectGo.Add(m_sceneSplines[index].gameObject);
                        Selection.objects = selectGo.ToArray();
                        Repaint();
                    }
                }
            }
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
            EditorGUILayout.EndScrollView();
            if(Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if (m_selected.Count > 0) m_selected = new List<int>(new int[] { m_selected[0] });
                    else m_selected[0]++;
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if (m_selected.Count > 0) m_selected = new List<int>(new int[] { m_selected[m_selected.Count - 1] });
                    else m_selected[0]++;
                }
                if (m_selected.Count == 0) return;
                if (m_selected[0] < 0) m_selected[0] = m_sceneSplines.Count - 1;
                if (m_selected[0] >= m_sceneSplines.Count) m_selected[0] = 0;
                if (m_sceneSplines.Count > 0) Selection.activeGameObject = m_sceneSplines[m_selected[0]].gameObject;
            }
        }
    }
}

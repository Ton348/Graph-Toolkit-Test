namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    public class SplineToolsWindow : EditorWindow
    {
        private static SplineTool[] s_tools;
        private int m_toolIndex = -1;
        private Vector2 m_scroll = Vector2.zero;
        private const float s_menuWidth = 150f;
        [MenuItem("Window/Dreamteck/Splines/Tools")]
        static void Init()
        {
            SplineToolsWindow window = (SplineToolsWindow)EditorWindow.GetWindow(typeof(SplineToolsWindow));
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Spline Tools");
            name = "Spline tools";
            autoRepaintOnSceneChange = true;

            List<Type> types = FindDerivedClasses.GetAllDerivedClasses(typeof(SplineTool));
            s_tools = new SplineTool[types.Count];
            int count = 0;
            foreach (Type t in types)
            {
                s_tools[count] = (SplineTool)Activator.CreateInstance(t);
                count++;
            } 
            if (m_toolIndex >= 0 && m_toolIndex < s_tools.Length) s_tools[m_toolIndex].Open(this);
        }

        void OnDestroy()
        {
            if (m_toolIndex >= 0 && m_toolIndex < s_tools.Length) s_tools[m_toolIndex].Close();
        }

        void OnGui()
        {
            if (s_tools == null) Awake(); 
            GUI.color = new Color(0f, 0f, 0f, 0.15f);
            GUI.DrawTexture(new Rect(0, 0, s_menuWidth, position.height), SplineEditorGui.white, ScaleMode.StretchToFill);
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.BeginScrollView(m_scroll, GUILayout.Width(s_menuWidth), GUILayout.Height(position.height-10));
            if (s_tools == null) Init();
            SplineEditorGui.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
            for (int i = 0; i < s_tools.Length; i ++)
            {
                if (SplineEditorGui.EditorLayoutSelectableButton(new GUIContent(s_tools[i].GetName()), true, m_toolIndex == i))
                {
                    if (m_toolIndex >= 0 && m_toolIndex < s_tools.Length) s_tools[m_toolIndex].Close();
                    m_toolIndex = i;
                    if (m_toolIndex < s_tools.Length) s_tools[m_toolIndex].Open(this);
                }
            }
            GUILayout.EndScrollView();

           
            if(m_toolIndex >= 0 && m_toolIndex < s_tools.Length)
            {
                GUILayout.BeginVertical();
                s_tools[m_toolIndex].Draw(new Rect(s_menuWidth, 0, position.width - s_menuWidth - 5f, position.height - 10));
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    static string s_showedReadmeSessionStateName = "ReadmeEditor.showedReadme";
    
    static string s_readmeSourceDirectory = "Assets/TutorialInfo";

    const float s_kSpace = 16f;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    static void RemoveTutorial()
    {
        if (EditorUtility.DisplayDialog("Remove Readme Assets",
            
            $"All contents under {s_readmeSourceDirectory} will be removed, are you sure you want to proceed?",
            "Proceed",
            "Cancel"))
        {
            if (Directory.Exists(s_readmeSourceDirectory))
            {
                FileUtil.DeleteFileOrDirectory(s_readmeSourceDirectory);
                FileUtil.DeleteFileOrDirectory(s_readmeSourceDirectory + ".meta");
            }

            var readmeAsset = SelectReadme();
            if (readmeAsset != null)
            {
                var path = AssetDatabase.GetAssetPath(readmeAsset);
                FileUtil.DeleteFileOrDirectory(path + ".meta");
                FileUtil.DeleteFileOrDirectory(path);
            }

            AssetDatabase.Refresh();
        }
    }

    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(s_showedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(s_showedReadmeSessionStateName, true);

            if (readme && !readme.loadedLayout)
            {
                LoadLayout();
                readme.loadedLayout = true;
            }
        }
    }

    static void LoadLayout()
    {
        var assembly = typeof(EditorApplication).Assembly;
        var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
    }

    static Readme SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Readme t:Readme");
        if (ids.Length == 1)
        {
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

            Selection.objects = new UnityEngine.Object[] { readmeObject };

            return (Readme)readmeObject;
        }
        else
        {
            return null;
        }
    }

    protected override void OnHeaderGUI()
    {
        var readme = (Readme)target;
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            if (readme.icon != null)
            {
                GUILayout.Space(s_kSpace);
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            }
            GUILayout.Space(s_kSpace);
            GUILayout.BeginVertical();
            {

                GUILayout.FlexibleSpace();
                GUILayout.Label(readme.title, titleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        var readme = (Readme)target;
        Init();

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, headingStyle);
            }

            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, bodyStyle);
            }

            if (!string.IsNullOrEmpty(section.linkText))
            {
                if (LinkLabel(new GUIContent(section.linkText)))
                {
                    Application.OpenURL(section.url);
                }
            }

            GUILayout.Space(s_kSpace);
        }

        if (GUILayout.Button("Remove Readme Assets", buttonStyle))
        {
            RemoveTutorial();
        }
    }

    bool m_initialized;

    GUIStyle linkStyle
    {
        get { return m_linkStyle; }
    }

    [SerializeField]
    GUIStyle m_linkStyle;

    GUIStyle titleStyle
    {
        get { return m_titleStyle; }
    }

    [SerializeField]
    GUIStyle m_titleStyle;

    GUIStyle headingStyle
    {
        get { return m_headingStyle; }
    }

    [SerializeField]
    GUIStyle m_headingStyle;

    GUIStyle bodyStyle
    {
        get { return m_bodyStyle; }
    }

    [SerializeField]
    GUIStyle m_bodyStyle;

    GUIStyle buttonStyle
    {
        get { return m_buttonStyle; }
    }

    [SerializeField]
    GUIStyle m_buttonStyle;

    void Init()
    {
        if (m_initialized)
            return;
        m_bodyStyle = new GUIStyle(EditorStyles.label);
        m_bodyStyle.wordWrap = true;
        m_bodyStyle.fontSize = 14;
        m_bodyStyle.richText = true;

        m_titleStyle = new GUIStyle(m_bodyStyle);
        m_titleStyle.fontSize = 26;

        m_headingStyle = new GUIStyle(m_bodyStyle);
        m_headingStyle.fontStyle = FontStyle.Bold;
        m_headingStyle.fontSize = 18;

        m_linkStyle = new GUIStyle(m_bodyStyle);
        m_linkStyle.wordWrap = false;

        // Match selection color which works nicely for both light and dark skins
        m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_linkStyle.stretchWidth = false;

        m_buttonStyle = new GUIStyle(EditorStyles.miniButton);
        m_buttonStyle.fontStyle = FontStyle.Bold;

        m_initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, linkStyle, options);

        Handles.BeginGUI();
        Handles.color = linkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, linkStyle);
    }
}
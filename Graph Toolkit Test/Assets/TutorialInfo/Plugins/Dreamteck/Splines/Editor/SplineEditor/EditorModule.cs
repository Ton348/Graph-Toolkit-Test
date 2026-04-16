namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class EditorModule
    {
        protected string m_prefPrefix = "";

        private bool m_changed = false;

        public bool hasChanged { get { return m_changed; } }

        protected SceneView m_currentSceneView;


        protected void RegisterChange()
        {
            m_changed = true;
        }

        public virtual void Select()
        {
            LoadState();
        }

        public virtual void Deselect()
        {
            SaveState();
        }

        public virtual void BeforeSceneDraw(SceneView current)
        {
            m_currentSceneView = current;
        }

        public void DrawScene()
        {
            m_changed = false;
            OnDrawScene();
        }

        protected virtual void OnDrawScene()
        {
        }

        public void DrawInspector()
        {
            m_changed = false;
            OnDrawInspector();
        }

        protected virtual void OnDrawInspector()
        {
        }

        public virtual GUIContent GetIconOff()
        {
            return new GUIContent("OFF", "Point Module Off");
        }

        public virtual GUIContent GetIconOn()
        {
            return new GUIContent("ON", "Point Module On");
        }

        protected virtual void RecordUndo(string title)
        {
        }

        protected virtual void Repaint()
        {
        }

        protected void SaveBool(string variableName, bool value)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            EditorPrefs.SetBool(m_prefPrefix + "." + variableName, value);
        }

        protected void SaveInt(string variableName, int value)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            EditorPrefs.SetInt(m_prefPrefix + "." + variableName, value);
        }

        protected void SaveFloat(string variableName, float value)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            EditorPrefs.SetFloat(m_prefPrefix + "." + variableName, value);
        }

        protected void SaveString(string variableName, string value)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            EditorPrefs.SetString(m_prefPrefix + "." + variableName, value);
        }

        protected bool LoadBool(string variableName)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            return EditorPrefs.GetBool(m_prefPrefix + "." + variableName, false);
        }

        protected int LoadInt(string variableName, int defaultValue = 0)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            return EditorPrefs.GetInt(m_prefPrefix + "." + variableName, defaultValue);
        }

        protected float LoadFloat(string variableName, float d = 0f)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            return EditorPrefs.GetFloat(m_prefPrefix + "." + variableName, d);
        }

        protected string LoadString(string variableName)
        {
            if (m_prefPrefix == "") m_prefPrefix = GetType().ToString();
            return EditorPrefs.GetString(m_prefPrefix + "." + variableName, "");
        }

        public virtual void SaveState()
        {

        }

        public virtual void LoadState()
        {

        }

        internal static GUIContent IconContent(string title, string iconName, string description)
        {
            GUIContent content = new GUIContent(title, description);
            if (EditorGUIUtility.isProSkin)
            {
                iconName += "_dark";
            }
            Texture2D tex = ResourceUtility.EditorLoadTexture("Splines/Editor/Icons", iconName);
            if (tex != null)
            {
                content.image = tex;
                content.text = "";
            }
            return content;
        }
    }
}

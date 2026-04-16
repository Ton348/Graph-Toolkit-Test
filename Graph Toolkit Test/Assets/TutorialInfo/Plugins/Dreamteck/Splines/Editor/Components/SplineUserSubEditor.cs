namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class SplineUserSubEditor
    {
        protected string m_title = "";
        protected SplineUser m_user;
        protected SplineUserEditor m_editor = null;
        public bool alwaysOpen = false;

        public bool isOpen
        {
            get { return m_foldout || alwaysOpen; }
        }
        bool m_foldout = false;

        public SplineUserSubEditor(SplineUser user, SplineUserEditor editor)
        {
            this.m_editor = editor;
            this.m_user = user;
        }

        public virtual void DrawInspector()
        {
            if (!alwaysOpen)
            {
                m_foldout = EditorGUILayout.Foldout(m_foldout, m_title);
            }
        }

        public virtual void DrawScene()
        {

        }
    }
}

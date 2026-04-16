namespace Dreamteck.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class Toolbar
    {
        GUIContent[] m_shownContent;
        GUIContent[] m_allContent;
        public bool center = true;
        public bool newLine = true;
        public float elementWidth = 0f;
        public float elementHeight = 23f;

        public Toolbar(GUIContent[] iconsNormal, GUIContent[] iconsSelected, float elementWidth = 0f)
        {
            this.elementWidth = elementWidth;
            if(iconsNormal.Length != iconsSelected.Length)
            {
                Debug.LogError("Invalid icon count for toolbar ");
                return;
            }
            m_allContent = new GUIContent[iconsNormal.Length * 2];
            m_shownContent = new GUIContent[iconsNormal.Length];
            iconsNormal.CopyTo(m_allContent, 0);
            iconsSelected.CopyTo(m_allContent, iconsNormal.Length);
        }

        public Toolbar(GUIContent[] contents, float elementWidth = 0f)
        {
            this.elementWidth = elementWidth;
            m_allContent = new GUIContent[contents.Length * 2];
            m_shownContent = new GUIContent[contents.Length];
            contents.CopyTo(m_allContent, 0);
            contents.CopyTo(m_allContent, contents.Length);
        }

        public void SetContent(int index, GUIContent content)
        {
            m_allContent[index] = content;
            m_allContent[m_shownContent.Length + index] = content;
        }

        public void SetContent(int index, GUIContent content, GUIContent contentSelected)
        {
            m_allContent[index] = content;
            m_allContent[m_shownContent.Length + index] = contentSelected;
        }

        public void Draw(ref int selected)
        {
            for (int i = 0; i < m_shownContent.Length; i++)
            {
                m_shownContent[i] = selected == i ? m_allContent[m_shownContent.Length + i] : m_allContent[i];
            }
            if(newLine) EditorGUILayout.BeginHorizontal();
            if(center) GUILayout.FlexibleSpace();
            if(elementWidth > 0f) selected = GUILayout.Toolbar(selected, m_shownContent, GUILayout.Width(elementWidth * m_shownContent.Length), GUILayout.Height(elementHeight));
            else selected = GUILayout.Toolbar(selected, m_shownContent, GUILayout.Height(elementHeight));
            if (center) GUILayout.FlexibleSpace();
            if (newLine) EditorGUILayout.EndHorizontal();
        }
    }
}

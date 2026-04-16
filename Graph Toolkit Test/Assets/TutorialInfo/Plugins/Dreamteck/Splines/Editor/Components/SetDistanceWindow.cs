namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    public class DistanceWindow : EditorWindow
    {
        float m_distance = 0f;
        DistanceReceiver m_rcv;
        float m_length = 0f;
        public delegate void DistanceReceiver(float distance);
        public void Init(DistanceReceiver receiver, float totalLength)
        {
            m_rcv = receiver;
            m_length = totalLength;
            titleContent = new GUIContent("Set Distance");
            minSize = maxSize = new Vector2(240, 90);
        }

        private void OnGui()
        {
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
            {
                m_rcv(m_distance);
                Close();
            }
            m_distance = EditorGUILayout.FloatField("Distance", m_distance);
            if (m_distance < 0f) m_distance = 0f;
            else if (m_distance > m_length) m_distance = m_length;
            if (m_distance > 0f)
            {
                EditorGUILayout.LabelField("Press Enter to set.", EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.HelpBox("Enter the distance and press Enter. Current spline length: " + m_length, MessageType.Info);
        }
    }
}

namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using UnityEditor;

    public class MeshScaleModifierEditor : SplineSampleModifierEditor
    {
        public bool allowSelection = true;
        private float m_addTime = 0f;

        public MeshScaleModifierEditor(MeshGenerator user, SplineUserEditor editor, int channelIndex) : base(user, editor, "_channels/["+channelIndex+"]/_scaleModifier")
        {
            m_title = "Scale Modifiers";
        }

        public void ClearSelection()
        {
            m_selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Scale"))
            {
                var key = AddKey(m_addTime - 0.1f, m_addTime + 0.1f);
                key.FindPropertyRelative("scale").vector3Value = Vector3.one;
                UpdateValues();
            }
        }

        protected override void KeyGui(SerializedProperty key)
        {
            SerializedProperty scale = key.FindPropertyRelative("scale");
            base.KeyGui(key);
            EditorGUILayout.PropertyField(scale);
        }
    }
}

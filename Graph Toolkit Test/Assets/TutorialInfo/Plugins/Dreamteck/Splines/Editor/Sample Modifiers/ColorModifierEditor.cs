namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using UnityEditor;

    public class ColorModifierEditor : SplineSampleModifierEditor
    {
        private float m_addTime = 0f;

        public ColorModifierEditor(SplineUser user, SplineUserEditor editor) : base(user, editor, "_colorModifier")
        {
            m_title = "Color Modifiers";
        }

        public void ClearSelection()
        {
            m_selected = -1;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (!isOpen) return;
            if (GUILayout.Button("Add New Color"))
            {
                AddKey(m_addTime - 0.1f, m_addTime + 0.1f);
                UpdateValues();
            }
        }

        protected override void KeyGui(SerializedProperty key)
        {
            SerializedProperty color = key.FindPropertyRelative("color");
            SerializedProperty blendMode = key.FindPropertyRelative("blendMode");
            base.KeyGui(key);
            EditorGUILayout.PropertyField(color);
            EditorGUILayout.PropertyField(blendMode);
        }
    }
}

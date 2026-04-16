using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class SizeModifierEditor : SplineSampleModifierEditor
	{
		private readonly float m_addTime = 0f;
		public bool allowSelection = true;

		public SizeModifierEditor(SplineUser user, SplineUserEditor editor) : base(user, editor, "_sizeModifier")
		{
			m_title = "Size Modifiers";
		}

		public void ClearSelection()
		{
			m_selected = -1;
		}

		public override void DrawInspector()
		{
			base.DrawInspector();
			if (!isOpen)
			{
				return;
			}

			if (GUILayout.Button("Add New Size"))
			{
				AddKey(m_addTime - 0.1f, m_addTime + 0.1f);
				UpdateValues();
			}
		}

		protected override void KeyGui(SerializedProperty key)
		{
			SerializedProperty size = key.FindPropertyRelative("size");
			base.KeyGui(key);
			EditorGUILayout.PropertyField(size);
		}
	}
}
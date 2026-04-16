using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class MeshScaleModifierEditor : SplineSampleModifierEditor
	{
		private readonly float m_addTime = 0f;
		public bool allowSelection = true;

		public MeshScaleModifierEditor(MeshGenerator user, SplineUserEditor editor, int channelIndex) : base(user,
			editor, "_channels/[" + channelIndex + "]/_scaleModifier")
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
			if (!isOpen)
			{
				return;
			}

			if (GUILayout.Button("Add New Scale"))
			{
				SerializedProperty key = AddKey(m_addTime - 0.1f, m_addTime + 0.1f);
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
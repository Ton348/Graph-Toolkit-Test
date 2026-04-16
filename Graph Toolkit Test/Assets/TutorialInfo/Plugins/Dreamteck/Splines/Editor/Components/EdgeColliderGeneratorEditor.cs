using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(EdgeColliderGenerator))]
	[CanEditMultipleObjects]
	public class EdgeColliderGeneratorEditor : SplineUserEditor
	{
		protected override void BodyGui()
		{
			base.BodyGui();
			var generator = (EdgeColliderGenerator)target;

			serializedObject.Update();
			SerializedProperty offset = serializedObject.FindProperty("_offset");
			SerializedProperty updateRate = serializedObject.FindProperty("updateRate");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Polygon", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(offset, new GUIContent("Offset"));
			EditorGUILayout.PropertyField(updateRate);
			if (updateRate.floatValue < 0f)
			{
				updateRate.floatValue = 0f;
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
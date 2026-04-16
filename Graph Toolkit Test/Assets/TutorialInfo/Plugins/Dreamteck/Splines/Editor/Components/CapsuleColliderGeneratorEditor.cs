using UnityEditor;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(CapsuleColliderGenerator), true)]
	[CanEditMultipleObjects]
	public class CapsuleColliderGeneratorEditor : SplineUserEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}

		protected override void BodyGui()
		{
			base.BodyGui();
			var generator = (CapsuleColliderGenerator)target;
			SerializedProperty directionProperty = serializedObject.FindProperty("_direction");
			SerializedProperty heightProperty = serializedObject.FindProperty("_height");
			SerializedProperty radiusProperty = serializedObject.FindProperty("_radius");
			SerializedProperty overlapCapsProperty = serializedObject.FindProperty("_overlapCaps");

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(directionProperty);
			var direction = (CapsuleColliderGenerator.CapsuleColliderZdirection)directionProperty.intValue;
			if (direction == CapsuleColliderGenerator.CapsuleColliderZdirection.Z)
			{
				EditorGUILayout.PropertyField(radiusProperty);
				EditorGUILayout.PropertyField(overlapCapsProperty);
			}
			else
			{
				EditorGUILayout.PropertyField(heightProperty);
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
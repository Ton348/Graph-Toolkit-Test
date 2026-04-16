using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(SplineProjector), true)]
	[CanEditMultipleObjects]
	public class SplineProjectorEditor : SplineTracerEditor
	{
		private bool m_info;

		public override void OnInspectorGUI()
		{
			var user = (SplineProjector)target;
			if (user.mode == SplineProjector.Mode.Accurate)
			{
				m_showAveraging = false;
			}
			else
			{
				m_showAveraging = true;
			}

			base.OnInspectorGUI();
		}

		protected override void BodyGui()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Projector", EditorStyles.boldLabel);

			serializedObject.Update();
			SerializedProperty mode = serializedObject.FindProperty("_mode");
			SerializedProperty projectTarget = serializedObject.FindProperty("_projectTarget");
			SerializedProperty targetObject = serializedObject.FindProperty("_targetObject");
			SerializedProperty autoProject = serializedObject.FindProperty("_autoProject");


			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(mode, new GUIContent("Mode"));
			if (mode.intValue == (int)SplineProjector.Mode.Accurate)
			{
				SerializedProperty subdivide = serializedObject.FindProperty("_subdivide");
				EditorGUILayout.PropertyField(subdivide, new GUIContent("Subdivide"));
			}

			EditorGUILayout.PropertyField(projectTarget, new GUIContent("Project Target"));
			EditorGUILayout.PropertyField(targetObject, new GUIContent("Apply Target"));

			GUI.color = Color.white;
			EditorGUILayout.PropertyField(autoProject, new GUIContent("Auto Project"));

			m_info = EditorGUILayout.Foldout(m_info, "Info");
			SerializedProperty percent = serializedObject.FindProperty("_result").FindPropertyRelative("percent");
			if (m_info)
			{
				EditorGUILayout.HelpBox("Projection percent: " + percent.floatValue, MessageType.Info);
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}

			base.BodyGui();
		}

		protected override void DuringSceneGui(SceneView currentSceneView)
		{
			base.DuringSceneGui(currentSceneView);
			for (var i = 0; i < m_users.Length; i++)
			{
				var user = (SplineProjector)m_users[i];
				if (user.spline == null)
				{
					return;
				}

				if (!user.autoProject)
				{
					return;
				}

				DrawResult(user.result);
			}
		}
	}
}
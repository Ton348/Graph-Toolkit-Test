using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class TransformModuleEditor : SplineUserSubEditor
	{
		private readonly TransformModule m_motionApplier;
		private readonly string[] m_toolStrings = { "3D", "2D" };

		public TransformModuleEditor(SplineUser user, SplineUserEditor parent, TransformModule input) : base(user,
			parent)
		{
			m_title = "Motion";
			m_motionApplier = input;
		}

		public override void DrawInspector()
		{
			base.DrawInspector();
			if (!isOpen)
			{
				return;
			}

			EditorGUI.indentLevel = 1;

			int selected = GUILayout.Toolbar(m_motionApplier.is2D ? 1 : 0, m_toolStrings);
			m_motionApplier.is2D = selected == 1;

			if (m_motionApplier.is2D)
			{
				m_motionApplier.applyPosition2D =
					EditorGUILayout.Toggle("Apply Position", m_motionApplier.applyPosition2D);
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Position", GUILayout.Width(EditorGUIUtility.labelWidth));
				m_motionApplier.applyPositionX =
					EditorGUILayout.Toggle(m_motionApplier.applyPositionX, GUILayout.Width(30));
				GUILayout.Label("X", GUILayout.Width(20));
				m_motionApplier.applyPositionY =
					EditorGUILayout.Toggle(m_motionApplier.applyPositionY, GUILayout.Width(30));
				GUILayout.Label("Y", GUILayout.Width(20));
				m_motionApplier.applyPositionZ =
					EditorGUILayout.Toggle(m_motionApplier.applyPositionZ, GUILayout.Width(30));
				GUILayout.Label("Z", GUILayout.Width(20));
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 150;
				m_motionApplier.retainLocalPosition =
					EditorGUILayout.Toggle("Retain Local Position", m_motionApplier.retainLocalPosition);
				EditorGUIUtility.labelWidth = 0;
				if (m_motionApplier.retainLocalPosition)
				{
					EditorGUILayout.HelpBox(
						"Retain Local Position is an experimental feature and will always accumulate an offset error based on how fast the follower is going.",
						MessageType.Info);
				}
			}

			if (m_motionApplier.applyPosition)
			{
				EditorGUI.indentLevel = 2;
				if (m_motionApplier.is2D)
				{
					Vector2 offset2d = m_motionApplier.offset;
					offset2d.y = EditorGUILayout.FloatField("Offset", offset2d.y);
					m_motionApplier.offset = offset2d;
				}
				else
				{
					m_motionApplier.offset = EditorGUILayout.Vector2Field("Offset", m_motionApplier.offset);
				}
			}

			EditorGUI.indentLevel = 1;

			if (m_motionApplier.is2D)
			{
				m_motionApplier.applyRotation2D =
					EditorGUILayout.Toggle("Apply Rotation", m_motionApplier.applyRotation2D);
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Rotation", GUILayout.Width(EditorGUIUtility.labelWidth));
				m_motionApplier.applyRotationX =
					EditorGUILayout.Toggle(m_motionApplier.applyRotationX, GUILayout.Width(30));
				GUILayout.Label("X", GUILayout.Width(20));
				m_motionApplier.applyRotationY =
					EditorGUILayout.Toggle(m_motionApplier.applyRotationY, GUILayout.Width(30));
				GUILayout.Label("Y", GUILayout.Width(20));
				m_motionApplier.applyRotationZ =
					EditorGUILayout.Toggle(m_motionApplier.applyRotationZ, GUILayout.Width(30));
				GUILayout.Label("Z", GUILayout.Width(20));
				EditorGUILayout.EndHorizontal();

				EditorGUIUtility.labelWidth = 150;
				m_motionApplier.retainLocalRotation =
					EditorGUILayout.Toggle("Retain Local Rotation", m_motionApplier.retainLocalRotation);
				EditorGUIUtility.labelWidth = 0;
				if (m_motionApplier.retainLocalRotation)
				{
					EditorGUILayout.HelpBox(
						"Retain Local Rotation is an experimental feature and will always accumulate an offset error based on how fast the follower is going.",
						MessageType.Info);
				}
			}

			if (m_motionApplier.applyRotation)
			{
				EditorGUI.indentLevel = 2;
				if (m_motionApplier.is2D)
				{
					Vector3 rot2d = m_motionApplier.rotationOffset;
					rot2d.z = EditorGUILayout.FloatField("Offset", rot2d.z);
					m_motionApplier.rotationOffset = rot2d;
				}
				else
				{
					m_motionApplier.rotationOffset =
						EditorGUILayout.Vector3Field("Offset", m_motionApplier.rotationOffset);
				}
			}

			EditorGUI.indentLevel = 1;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Scale", GUILayout.Width(EditorGUIUtility.labelWidth));
			m_motionApplier.applyScaleX = EditorGUILayout.Toggle(m_motionApplier.applyScaleX, GUILayout.Width(30));
			GUILayout.Label("X", GUILayout.Width(20));
			m_motionApplier.applyScaleY = EditorGUILayout.Toggle(m_motionApplier.applyScaleY, GUILayout.Width(30));
			GUILayout.Label("Y", GUILayout.Width(20));
			m_motionApplier.applyScaleZ = EditorGUILayout.Toggle(m_motionApplier.applyScaleZ, GUILayout.Width(30));
			GUILayout.Label("Z", GUILayout.Width(20));
			EditorGUILayout.EndHorizontal();

			if (m_motionApplier.applyScale)
			{
				EditorGUI.indentLevel = 2;
				m_motionApplier.baseScale = EditorGUILayout.Vector3Field("Base scale", m_motionApplier.baseScale);
			}

			m_motionApplier.velocityHandleMode =
				(TransformModule.VelocityHandleMode)EditorGUILayout.EnumPopup("Velocity Mode",
					m_motionApplier.velocityHandleMode);
			EditorGUI.indentLevel = 0;
		}
	}
}
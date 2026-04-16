using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class RotationModifierEditor : SplineSampleModifierEditor
	{
		private readonly float m_addTime = 0f;
		public bool allowSelection = true;

		public RotationModifierEditor(SplineUser user, SplineUserEditor parent) : base(user, parent,
			"_rotationModifier")
		{
			m_title = "Rotation Modifiers";
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

			if (GUILayout.Button("Add New Rotation"))
			{
				AddKey(m_addTime - 0.1f, m_addTime + 0.1f);
				UpdateValues();
			}
		}

		protected override void KeyGui(SerializedProperty key)
		{
			SerializedProperty rotation = key.FindPropertyRelative("rotation");
			SerializedProperty target = key.FindPropertyRelative("target");
			SerializedProperty useLookTarget = key.FindPropertyRelative("useLookTarget");
			base.KeyGui(key);
			if (!useLookTarget.boolValue)
			{
				EditorGUILayout.PropertyField(rotation);
			}

			EditorGUILayout.PropertyField(useLookTarget);
			if (useLookTarget.boolValue)
			{
				EditorGUILayout.PropertyField(target);
			}
		}

		protected override bool KeyHandles(SerializedProperty key, bool edit)
		{
			if (!isOpen)
			{
				return false;
			}

			var changed = false;
			SerializedProperty start = key.FindPropertyRelative("_featherStart");
			SerializedProperty end = key.FindPropertyRelative("_featherEnd");
			SerializedProperty centerStart = key.FindPropertyRelative("_centerStart");
			SerializedProperty centerEnd = key.FindPropertyRelative("_centerEnd");
			SerializedProperty rotation = key.FindPropertyRelative("rotation");
			SerializedProperty target = key.FindPropertyRelative("target");
			SerializedProperty useLookTarget = key.FindPropertyRelative("useLookTarget");
			float position = GetPosition(start.floatValue, end.floatValue, centerStart.floatValue,
				centerEnd.floatValue);
			var result = new SplineSample();
			m_user.spline.Evaluate(position, ref result);
			if (useLookTarget.boolValue)
			{
				if (target.objectReferenceValue != null)
				{
					var targetTransform = (Transform)target.objectReferenceValue;
					Handles.DrawDottedLine(result.position, targetTransform.position, 5f);
					if (edit)
					{
						Vector3 lastPos = targetTransform.position;
						targetTransform.position =
							Handles.PositionHandle(targetTransform.position, Quaternion.identity);
						if (lastPos != targetTransform.position)
						{
							MainPointModule.HoldInteraction();
							EditorUtility.SetDirty(targetTransform);
							changed = true;
						}
					}
				}
			}
			else
			{
				Quaternion directionRot = Quaternion.LookRotation(result.forward, result.up);
				Quaternion rot = directionRot * Quaternion.Euler(rotation.vector3Value);
				SplineEditorHandles.DrawArrowCap(result.position, rot, HandleUtility.GetHandleSize(result.position));

				if (edit)
				{
					Vector3 lastEuler = rot.eulerAngles;
					rot = Handles.RotationHandle(rot, result.position);
					rot = Quaternion.Inverse(directionRot) * rot;
					rotation.vector3Value = rot.eulerAngles;
					if (rot.eulerAngles != lastEuler)
					{
						MainPointModule.HoldInteraction();
						changed = true;
					}
				}
			}

			return base.KeyHandles(key, edit) || changed;
		}
	}
}
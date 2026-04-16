using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class SplineSampleModifierEditor : SplineUserSubEditor
	{
		protected SerializedProperty m_blend;
		private int m_deleteElement = -1;
		protected bool m_drawAllKeys;
		protected SerializedProperty m_keys;
		protected SerializedProperty m_modifier;
		protected int m_selected = -1;

		protected SerializedObject m_serializedObject;
		protected SerializedProperty m_useClipped;

		public SplineSampleModifierEditor(
			SplineUser user,
			SplineUserEditor editor,
			string modifierPropertyPath = "") : base(user, editor)
		{
			m_title = "Sample Modifier";
			m_serializedObject = new SerializedObject(user);
			string[] propertyPath = modifierPropertyPath.Split('/');
			SerializedProperty property = m_serializedObject.FindProperty(propertyPath[0]);
			for (var i = 1; i < propertyPath.Length; i++)
			{
				if (propertyPath[i].StartsWith("[") && propertyPath[i].EndsWith("]"))
				{
					string num = propertyPath[i].Substring(1, propertyPath[i].Length - 2);
					property = property.GetArrayElementAtIndex(int.Parse(num));
					continue;
				}

				property = property.FindPropertyRelative(propertyPath[i]);
			}

			m_modifier = property;
			m_keys = m_modifier.FindPropertyRelative(keysPropertyName);
			m_blend = m_modifier.FindPropertyRelative("blend");
			m_useClipped = m_modifier.FindPropertyRelative("useClippedPercent");
		}

		protected virtual SerializedProperty keysProperty => m_keys;
		protected virtual string keysPropertyName => "keys";

		public override void DrawInspector()
		{
			base.DrawInspector();
			if (!isOpen)
			{
				return;
			}

			if (keysProperty.arraySize > 0)
			{
				m_drawAllKeys = EditorGUILayout.Toggle("Draw all Modules", m_drawAllKeys);
			}

			m_serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			for (var i = 0; i < keysProperty.arraySize; i++)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(i);
				if (m_selected == i)
				{
					EditorGUI.BeginChangeCheck();
					KeyGui(keyProperty);
					if (EditorGUI.EndChangeCheck())
					{
						m_user.Rebuild();
					}
				}
				else
				{
					SerializedProperty start = keyProperty.FindPropertyRelative("_featherStart");
					SerializedProperty end = keyProperty.FindPropertyRelative("_featherEnd");
					EditorGUILayout.LabelField(i + " [" + Mathf.Round(start.floatValue * 10) / 10f + " - " +
					                           Mathf.Round(end.floatValue * 10) / 10f + "]");
				}

				EditorGUILayout.EndVertical();
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					if (Event.current.type == EventType.MouseDown)
					{
						if (Event.current.button == 0)
						{
							m_selected = i;
							m_editor.Repaint();
						}
						else if (Event.current.button == 1)
						{
							int index = i;
							var menu = new GenericMenu();
							menu.AddItem(new GUIContent("Delete"), false, delegate { m_deleteElement = index; });
							menu.ShowAsContext();
							UpdateValues();
							m_serializedObject.Update();
						}
					}
				}
			}

			EditorGUILayout.Space();
			if (keysProperty.arraySize > 0)
			{
				EditorGUILayout.PropertyField(m_blend);
				EditorGUILayout.PropertyField(m_useClipped,
					new GUIContent("Use Clipped Percents",
						"Whether the percentages relate to the clip range of the user or not."));
			}

			if (m_deleteElement >= 0)
			{
				keysProperty.DeleteArrayElementAtIndex(m_deleteElement);
				m_deleteElement = -1;
				UpdateValues();
			}
			else
			{
				if (EditorGUI.EndChangeCheck())
				{
					UpdateValues();
				}
			}
		}

		public override void DrawScene()
		{
			base.DrawScene();
			m_serializedObject.Update();
			var changed = false;
			for (var i = 0; i < keysProperty.arraySize; i++)
			{
				if (m_selected == i || m_drawAllKeys)
				{
					if (KeyHandles(keysProperty.GetArrayElementAtIndex(i), m_selected == i))
					{
						changed = true;
					}
				}
			}

			if (changed)
			{
				UpdateValues();
			}
		}

		protected void UpdateValues()
		{
			if (m_serializedObject != null)
			{
				m_serializedObject.ApplyModifiedProperties();
			}

			m_user.Rebuild();
			m_editor.Repaint();
		}

		protected virtual SerializedProperty AddKey(float f, float t)
		{
			m_keys.InsertArrayElementAtIndex(m_keys.arraySize);
			SerializedProperty key = m_keys.GetArrayElementAtIndex(m_keys.arraySize - 1);
			SerializedProperty start = key.FindPropertyRelative("_featherStart");
			SerializedProperty end = key.FindPropertyRelative("_featherEnd");
			SerializedProperty centerStart = key.FindPropertyRelative("_centerStart");
			SerializedProperty centerEnd = key.FindPropertyRelative("_centerEnd");
			SerializedProperty blend = key.FindPropertyRelative("blend");
			SerializedProperty interpolation = key.FindPropertyRelative("interpolation");

			start.floatValue = Mathf.Clamp01(f);
			end.floatValue = Mathf.Clamp01(t);
			blend.floatValue = 1f;
			interpolation.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			centerStart.floatValue = 0.1f;
			centerEnd.floatValue = 0.9f;
			return key;
		}

		protected virtual void KeyGui(SerializedProperty keyProperty)
		{
			SerializedProperty start = keyProperty.FindPropertyRelative("_featherStart");
			SerializedProperty end = keyProperty.FindPropertyRelative("_featherEnd");
			SerializedProperty centerStart = keyProperty.FindPropertyRelative("_centerStart");
			SerializedProperty centerEnd = keyProperty.FindPropertyRelative("_centerEnd");
			SerializedProperty interpolation = keyProperty.FindPropertyRelative("interpolation");
			SerializedProperty blend = keyProperty.FindPropertyRelative("blend");

			EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth = 50f;
			start.floatValue = EditorGUILayout.Slider("Start", start.floatValue, 0f, 1f);
			end.floatValue = EditorGUILayout.Slider("End", end.floatValue, 0f, 1f);
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0f;
			float cs = centerStart.floatValue;
			float ce = centerEnd.floatValue;
			EditorGUILayout.MinMaxSlider("Center", ref cs, ref ce, 0f, 1f);
			centerStart.floatValue = cs;
			centerEnd.floatValue = ce;
			EditorGUILayout.PropertyField(interpolation);
			EditorGUILayout.PropertyField(blend);
		}

		protected static float GlobalToLocalPercent(float start, float end, float t)
		{
			if (start > end)
			{
				if (t > start)
				{
					return Mathf.InverseLerp(start, start + (1f - start) + end, t);
				}

				if (t < end)
				{
					return Mathf.InverseLerp(-(1f - start), end, t);
				}

				return 0f;
			}

			return Mathf.InverseLerp(start, end, t);
		}

		protected static float LocalToGlobalPercent(float start, float end, float t)
		{
			if (start > end)
			{
				t = Mathf.Lerp(start, start + (1f - start) + end, t);
				if (t > 1f)
				{
					t -= Mathf.Floor(t);
				}

				return t;
			}

			return Mathf.Lerp(start, end, t);
		}

		protected static float GetPosition(float start, float end, float centerStart, float centerEnd)
		{
			float center = Mathf.Lerp(centerStart, centerEnd, 0.5f);
			if (start > end)
			{
				float fromToEndDistance = 1f - start;
				float centerDistance = center * (fromToEndDistance + end);
				float pos = start + centerDistance;
				if (pos > 1f)
				{
					pos -= Mathf.Floor(pos);
				}

				return pos;
			}

			return Mathf.Lerp(start, end, center);
		}

		protected virtual bool KeyHandles(SerializedProperty key, bool edit)
		{
			if (!isOpen)
			{
				return false;
			}

			bool useClip = m_useClipped.boolValue;

			SerializedProperty start = key.FindPropertyRelative("_featherStart");
			SerializedProperty end = key.FindPropertyRelative("_featherEnd");
			SerializedProperty centerStart = key.FindPropertyRelative("_centerStart");
			SerializedProperty centerEnd = key.FindPropertyRelative("_centerEnd");

			var changed = false;
			double value = start.floatValue;

			if (useClip)
			{
				m_user.UnclipPercent(ref value);
			}

			SplineComputerEditorHandles.Slider(m_user.spline, ref value, m_user.spline.editorPathColor, "Start",
				SplineComputerEditorHandles.SplineSliderGizmo.ForwardTriangle, 0.8f);
			if (useClip)
			{
				m_user.ClipPercent(ref value);
			}

			if (start.floatValue != value)
			{
				MainPointModule.HoldInteraction();
				start.floatValue = (float)value;
				changed = true;
			}

			value = LocalToGlobalPercent(start.floatValue, end.floatValue, centerStart.floatValue);
			if (useClip)
			{
				m_user.UnclipPercent(ref value);
			}

			SplineComputerEditorHandles.Slider(m_user.spline, ref value, m_user.spline.editorPathColor, "",
				SplineComputerEditorHandles.SplineSliderGizmo.Rectangle, 0.6f);
			if (useClip)
			{
				m_user.ClipPercent(ref value);
			}

			if (LocalToGlobalPercent(start.floatValue, end.floatValue, centerStart.floatValue) != value)
			{
				MainPointModule.HoldInteraction();
				centerStart.floatValue = GlobalToLocalPercent(start.floatValue, end.floatValue, (float)value);
				changed = true;
			}

			value = LocalToGlobalPercent(start.floatValue, end.floatValue, centerEnd.floatValue);
			if (useClip)
			{
				m_user.UnclipPercent(ref value);
			}


			SplineComputerEditorHandles.Slider(m_user.spline, ref value, m_user.spline.editorPathColor, "",
				SplineComputerEditorHandles.SplineSliderGizmo.Rectangle, 0.6f);
			if (useClip)
			{
				m_user.ClipPercent(ref value);
			}

			if (LocalToGlobalPercent(start.floatValue, end.floatValue, centerEnd.floatValue) != value)
			{
				MainPointModule.HoldInteraction();
				centerEnd.floatValue = GlobalToLocalPercent(start.floatValue, end.floatValue, (float)value);
				changed = true;
			}


			value = end.floatValue;
			if (useClip)
			{
				m_user.UnclipPercent(ref value);
			}

			SplineComputerEditorHandles.Slider(m_user.spline, ref value, m_user.spline.editorPathColor, "End",
				SplineComputerEditorHandles.SplineSliderGizmo.BackwardTriangle, 0.8f);
			if (useClip)
			{
				m_user.ClipPercent(ref value);
			}

			if (end.floatValue != value)
			{
				MainPointModule.HoldInteraction();
				end.floatValue = (float)value;
				changed = true;
			}


			if (Event.current.control)
			{
				value = GetPosition(start.floatValue, end.floatValue, centerStart.floatValue, centerEnd.floatValue);
				double lastValue = value;
				if (useClip)
				{
					m_user.UnclipPercent(ref value);
				}

				SplineComputerEditorHandles.Slider(m_user.spline, ref value, m_user.spline.editorPathColor, "",
					SplineComputerEditorHandles.SplineSliderGizmo.Circle, 0.4f);

				if (useClip)
				{
					m_user.ClipPercent(ref value);
				}

				if (value != lastValue)
				{
					MainPointModule.HoldInteraction();
					double delta = value - lastValue;
					start.floatValue += (float)delta;
					end.floatValue += (float)delta;
					start.floatValue = Mathf.Clamp01(start.floatValue);
					end.floatValue = Mathf.Clamp(end.floatValue, start.floatValue, 1f);
					changed = true;
				}
			}

			return changed;
		}
	}
}
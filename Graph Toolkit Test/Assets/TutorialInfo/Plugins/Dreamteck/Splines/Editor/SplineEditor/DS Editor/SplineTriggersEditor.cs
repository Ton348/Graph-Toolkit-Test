using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class SplineTriggersEditor : SplineEditorBase
	{
		private readonly SplineComputer m_spline;
		private SplineTrigger.Type m_addTriggerType = SplineTrigger.Type.Double;
		private bool m_renameTrigger, m_renameGroup;
		private int m_selected = -1, m_selectedGroup = -1;
		private int m_setDistanceGroup, m_setDistanceTrigger;

		public SplineTriggersEditor(SplineComputer spline, SerializedObject serializedObject) : base(serializedObject)
		{
			m_spline = spline;
		}

		protected override void Load()
		{
			base.Load();
			m_addTriggerType = (SplineTrigger.Type)LoadInt("addTriggerType");
		}

		protected override void Save()
		{
			base.Save();
			SaveInt("addTriggerType", (int)m_addTriggerType);
		}

		public override void DrawInspector()
		{
			base.DrawInspector();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginVertical();
			for (var i = 0; i < m_spline.triggerGroups.Length; i++)
			{
				DrawGroupGui(i);
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("New Group"))
			{
				RecordUndo("Add Trigger Group");
				var group = new TriggerGroup();
				group.name = "Trigger Group " + (m_spline.triggerGroups.Length + 1);
				UnityEditor.ArrayUtility.Add(ref m_spline.triggerGroups, group);
			}

			EditorGUILayout.EndVertical();
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		public override void DrawScene(SceneView current)
		{
			base.DrawScene(current);

			if (m_spline == null)
			{
				return;
			}

			for (var i = 0; i < m_spline.triggerGroups.Length; i++)
			{
				if (!m_spline.triggerGroups[i].open)
				{
					continue;
				}

				DrawGroupScene(i);
			}
		}

		private void DrawGroupScene(int index)
		{
			TriggerGroup group = m_spline.triggerGroups[index];
			for (var i = 0; i < group.triggers.Length; i++)
			{
				var gizmo = SplineComputerEditorHandles.SplineSliderGizmo.DualArrow;
				switch (group.triggers[i].type)
				{
					case SplineTrigger.Type.Backward:
						gizmo = SplineComputerEditorHandles.SplineSliderGizmo.BackwardTriangle; break;
					case SplineTrigger.Type.Forward:
						gizmo = SplineComputerEditorHandles.SplineSliderGizmo.ForwardTriangle; break;
					case SplineTrigger.Type.Double:
						gizmo = SplineComputerEditorHandles.SplineSliderGizmo.DualArrow; break;
				}

				double last = group.triggers[i].position;
				if (SplineComputerEditorHandles.Slider(m_spline, ref group.triggers[i].position,
					    group.triggers[i].color, group.triggers[i].name, gizmo) || last != group.triggers[i].position)
				{
					Select(index, i);
					Repaint();
				}
			}
		}

		private void OnSetDistance(float distance)
		{
			var serializedObject = new SerializedObject(m_spline);
			SerializedProperty groups = serializedObject.FindProperty("triggerGroups");
			SerializedProperty groupProperty = groups.GetArrayElementAtIndex(m_setDistanceGroup);

			SerializedProperty triggersProperty = groupProperty.FindPropertyRelative("triggers");
			SerializedProperty triggerProperty = triggersProperty.GetArrayElementAtIndex(m_setDistanceTrigger);

			SerializedProperty position = triggerProperty.FindPropertyRelative("position");

			double travel = m_spline.Travel(0.0, distance);
			position.floatValue = (float)travel;
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawGroupGui(int index)
		{
			TriggerGroup group = m_spline.triggerGroups[index];
			var serializedObject = new SerializedObject(m_spline);
			SerializedProperty groups = serializedObject.FindProperty("triggerGroups");
			SerializedProperty groupProperty = groups.GetArrayElementAtIndex(index);
			EditorGUI.indentLevel += 2;
			if (m_selectedGroup == index && m_renameGroup)
			{
				if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return ||
				                                                Event.current.keyCode == KeyCode.KeypadEnter))
				{
					m_renameGroup = false;
					Repaint();
				}

				group.name = EditorGUILayout.TextField(group.name);
			}
			else
			{
				group.open = EditorGUILayout.Foldout(group.open, index + " - " + group.name);
			}

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
			    Event.current.button == 1)
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Rename"), false, delegate
				{
					RecordUndo("Rename Trigger Group");
					m_selectedGroup = index;
					m_renameGroup = true;
					m_renameTrigger = false;
					Repaint();
				});
				menu.AddItem(new GUIContent("Delete"), false, delegate
				{
					RecordUndo("Delete Trigger Group");
					UnityEditor.ArrayUtility.RemoveAt(ref m_spline.triggerGroups, index);
					Repaint();
				});
				menu.ShowAsContext();
			}

			EditorGUI.indentLevel -= 2;
			if (!group.open)
			{
				return;
			}

			for (var i = 0; i < group.triggers.Length; i++)
			{
				DrawTriggerGui(i, index, groupProperty);
			}

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Trigger"))
			{
				RecordUndo("Add Trigger");
				var newTrigger = new SplineTrigger(m_addTriggerType);
				newTrigger.name = "Trigger " + (group.triggers.Length + 1);
				UnityEditor.ArrayUtility.Add(ref group.triggers, newTrigger);
			}

			m_addTriggerType = (SplineTrigger.Type)EditorGUILayout.EnumPopup(m_addTriggerType);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		private void Select(int group, int trigger)
		{
			m_selected = trigger;
			m_selectedGroup = group;
			m_renameTrigger = false;
			m_renameGroup = false;
			Repaint();
		}

		private void DrawTriggerGui(int index, int groupIndex, SerializedProperty groupProperty)
		{
			bool isSelected = m_selected == index && m_selectedGroup == groupIndex;
			TriggerGroup group = m_spline.triggerGroups[groupIndex];
			SplineTrigger trigger = group.triggers[index];
			SerializedProperty triggersProperty = groupProperty.FindPropertyRelative("triggers");
			SerializedProperty triggerProperty = triggersProperty.GetArrayElementAtIndex(index);
			SerializedProperty eventProperty = triggerProperty.FindPropertyRelative("onCross");
			SerializedProperty positionProperty = triggerProperty.FindPropertyRelative("position");
			SerializedProperty colorProperty = triggerProperty.FindPropertyRelative("color");
			SerializedProperty nameProperty = triggerProperty.FindPropertyRelative("name");
			SerializedProperty enabledProperty = triggerProperty.FindPropertyRelative("enabled");
			SerializedProperty workOnceProperty = triggerProperty.FindPropertyRelative("workOnce");
			SerializedProperty typeProperty = triggerProperty.FindPropertyRelative("type");

			Color col = colorProperty.colorValue;
			if (isSelected)
			{
				col.a = 1f;
			}
			else
			{
				col.a = 0.6f;
			}

			GUI.backgroundColor = col;

			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUI.backgroundColor = Color.white;
			if (trigger == null)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("NULL");
				if (GUILayout.Button("x"))
				{
					UnityEditor.ArrayUtility.RemoveAt(ref group.triggers, index);
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
				return;
			}


			if (isSelected && m_renameTrigger)
			{
				if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return ||
				                                                Event.current.keyCode == KeyCode.KeypadEnter))
				{
					m_renameTrigger = false;
					Repaint();
				}

				nameProperty.stringValue = EditorGUILayout.TextField(nameProperty.stringValue);
			}
			else
			{
				EditorGUILayout.LabelField(nameProperty.stringValue);
			}

			if (isSelected)
			{
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(enabledProperty);
				EditorGUILayout.PropertyField(colorProperty);

				EditorGUILayout.BeginHorizontal();
				positionProperty.floatValue = EditorGUILayout.Slider("Position", positionProperty.floatValue, 0f, 1f);
				if (GUILayout.Button("Set Distance", GUILayout.Width(85)))
				{
					var w = EditorWindow.GetWindow<DistanceWindow>(true);
					w.Init(OnSetDistance, m_spline.CalculateLength());
					m_setDistanceGroup = groupIndex;
					m_setDistanceTrigger = index;
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.PropertyField(typeProperty);
				EditorGUILayout.PropertyField(workOnceProperty);

				EditorGUILayout.PropertyField(eventProperty);
			}

			EditorGUILayout.EndVertical();

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					Select(groupIndex, index);
				}
				else if (Event.current.button == 1)
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Deselect"), false, delegate { Select(-1, -1); });
					menu.AddItem(new GUIContent("Rename"), false, delegate
					{
						Select(groupIndex, index);
						m_renameTrigger = true;
						m_renameGroup = false;
					});
					if (index > 0)
					{
						menu.AddItem(new GUIContent("Move Up"), false, delegate
						{
							RecordUndo("Move Trigger Up");
							SplineTrigger temp = group.triggers[index - 1];
							group.triggers[index - 1] = trigger;
							group.triggers[index] = temp;
							m_selected--;
							m_renameTrigger = false;
						});
					}
					else
					{
						menu.AddDisabledItem(new GUIContent("Move Up"));
					}

					if (index < group.triggers.Length - 1)
					{
						menu.AddItem(new GUIContent("Move Down"), false, delegate
						{
							RecordUndo("Move Trigger Down");
							SplineTrigger temp = group.triggers[index + 1];
							group.triggers[index + 1] = trigger;
							group.triggers[index] = temp;
							m_selected--;
							m_renameTrigger = false;
						});
					}
					else
					{
						menu.AddDisabledItem(new GUIContent("Move Down"));
					}

					menu.AddItem(new GUIContent("Duplicate"), false, delegate
					{
						RecordUndo("Duplicate Trigger");
						var newTrigger = new SplineTrigger(SplineTrigger.Type.Double);
						newTrigger.color = colorProperty.colorValue;
						newTrigger.enabled = enabledProperty.boolValue;
						newTrigger.position = positionProperty.floatValue;
						newTrigger.type = (SplineTrigger.Type)typeProperty.intValue;
						newTrigger.name = "Trigger " + (group.triggers.Length + 1);
						UnityEditor.ArrayUtility.Add(ref group.triggers, newTrigger);
						Select(groupIndex, group.triggers.Length - 1);
					});
					menu.AddItem(new GUIContent("Delete"), false, delegate
					{
						RecordUndo("Delete Trigger");
						UnityEditor.ArrayUtility.RemoveAt(ref group.triggers, index);
						Select(-1, -1);
					});
					menu.ShowAsContext();
				}
			}
		}
	}
}
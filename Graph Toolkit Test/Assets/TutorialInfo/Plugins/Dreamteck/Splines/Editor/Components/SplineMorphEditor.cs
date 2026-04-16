using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(SplineMorph))]
	public class SplineMorphEditor : UnityEditor.Editor
	{
		private string m_addName = "";

		private SplineMorph m_morph;
		private bool m_rename;
		private int m_selected = -1;

		private void OnEnable()
		{
			m_morph = (SplineMorph)target;
			GetAddName();
		}

		private void GetAddName()
		{
			m_addName = "Channel " + m_morph.GetChannelCount();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			Undo.RecordObject(m_morph, "Edit Morph");
			m_morph.spline =
				(SplineComputer)EditorGUILayout.ObjectField("Spline", m_morph.spline, typeof(SplineComputer), true);
			m_morph.space = (SplineComputer.Space)EditorGUILayout.EnumPopup("Space", m_morph.space);
			m_morph.cycle = EditorGUILayout.Toggle("Runtime Cycle", m_morph.cycle);
			if (m_morph.cycle)
			{
				EditorGUI.indentLevel++;
				m_morph.cycleMode = (SplineMorph.CycleMode)EditorGUILayout.EnumPopup("Cycle Wrap", m_morph.cycleMode);
				m_morph.cycleUpdateMode =
					(SplineMorph.UpdateMode)EditorGUILayout.EnumPopup("Update Mode", m_morph.cycleUpdateMode);
				m_morph.cycleDuration = EditorGUILayout.FloatField("Cycle Duration", m_morph.cycleDuration);
				EditorGUI.indentLevel--;
			}

			int channelCount = m_morph.GetChannelCount();
			if (channelCount > 0)
			{
				if (m_morph.spline == null)
				{
					EditorGUILayout.HelpBox("No spline assigned.", MessageType.Error);
					return;
				}

				if (m_morph.GetSnapshot(0).Length != m_morph.spline.pointCount)
				{
					EditorGUILayout.HelpBox(
						"Recorded morphs require the spline to have " + m_morph.GetSnapshot(0).Length +
						". The spline has " + m_morph.spline.pointCount, MessageType.Error);
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Clear morph states"))
					{
						if (EditorUtility.DisplayDialog("Clear morph states?", "Do you want to clear all morph states?",
							    "Yes", "No"))
						{
							m_morph.Clear();
						}
					}

					var str = "Reduce";
					if (m_morph.GetSnapshot(0).Length > m_morph.spline.pointCount)
					{
						str = "Increase";
					}

					if (GUILayout.Button(str + " spline points"))
					{
						if (EditorUtility.DisplayDialog(str + " spline points?",
							    "Do you want to " + str + " the spline points?", "Yes", "No"))
						{
							m_morph.spline.SetPoints(m_morph.GetSnapshot(0), SplineComputer.Space.Local);
						}
					}

					if (GUILayout.Button("Update Morph States"))
					{
						if (EditorUtility.DisplayDialog("Update morph states?",
							    "This will add or delete the needed spline points to all morph states", "Yes", "No"))
						{
							for (var i = 0; i < m_morph.GetChannelCount(); i++)
							{
								SplinePoint[] points = m_morph.GetSnapshot(i);
								while (points.Length < m_morph.spline.pointCount)
								{
									ArrayUtility.Add(ref points, new SplinePoint());
								}

								while (points.Length > m_morph.spline.pointCount)
								{
									ArrayUtility.RemoveAt(ref points, points.Length - 1);
								}

								m_morph.SetSnapshot(i, points);
							}
						}
					}

					EditorGUILayout.EndHorizontal();
					return;
				}
			}

			for (var i = 0; i < channelCount; i++)
			{
				DrawChannel(i);
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+", GUILayout.Width(40)))
			{
				m_morph.AddChannel(m_addName);
				GetAddName();
			}

			m_addName = EditorGUILayout.TextField(m_addName);

			EditorGUILayout.EndHorizontal();
			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		private void DrawChannel(int index)
		{
			SplineMorph.Channel channel = m_morph.GetChannel(index);
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.backgroundColor = Color.white;
			if (m_selected == index && m_rename)
			{
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
				{
					m_rename = false;
				}

				channel.name = EditorGUILayout.TextField(channel.name);
			}
			else if (index > 0)
			{
				float weight = m_morph.GetWeight(index);
				float lastWeight = weight;
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("●", "Capture Snapshot"), GUILayout.Width(22f)))
				{
					m_morph.CaptureSnapshot(index);
				}

				EditorGUILayout.LabelField(channel.name, GUILayout.Width(EditorGUIUtility.labelWidth));
				weight = EditorGUILayout.Slider(weight, 0f, 1f);
				EditorGUILayout.EndHorizontal();
				if (lastWeight != weight)
				{
					m_morph.SetWeight(index, weight);
				}

				SplineMorph.Channel.Interpolation lastInterpolation = channel.interpolation;
				channel.interpolation =
					(SplineMorph.Channel.Interpolation)EditorGUILayout.EnumPopup("Interpolation",
						channel.interpolation);
				if (lastInterpolation != channel.interpolation)
				{
					m_morph.UpdateMorph();
				}

				channel.curve = EditorGUILayout.CurveField("Curve", channel.curve);
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("●", "Capture Snapshot"), GUILayout.Width(22f)))
				{
					m_morph.CaptureSnapshot(index);
				}

				GUILayout.Label(channel.name);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			Rect last = GUILayoutUtility.GetLastRect();
			if (last.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDown)
				{
					if (Event.current.button == 0)
					{
						m_rename = false;
						m_selected = -1;
						Repaint();
					}

					if (Event.current.button == 1)
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Rename"), false, delegate
						{
							m_rename = true;
							m_selected = index;
						});
						menu.AddItem(new GUIContent("Delete"), false, delegate
						{
							m_morph.SetWeight(index, 0f);
							m_morph.RemoveChannel(index);
							m_selected = -1;
							GetAddName();
						});
						menu.ShowAsContext();
					}
				}
			}
		}
	}
}
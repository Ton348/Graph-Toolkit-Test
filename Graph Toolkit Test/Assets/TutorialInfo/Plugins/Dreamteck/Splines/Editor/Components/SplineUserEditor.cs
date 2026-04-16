using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(SplineUser), true)]
	[CanEditMultipleObjects]
	public class SplineUserEditor : UnityEditor.Editor
	{
		protected bool m_showClip = true;
		protected bool m_showAveraging = true;
		protected bool m_showUpdateMethod = true;
		protected bool m_showMultithreading = true;
		private bool m_settingsFoldout;
		protected RotationModifierEditor m_rotationModifierEditor;
		protected OffsetModifierEditor m_offsetModifierEditor;
		protected ColorModifierEditor m_colorModifierEditor;
		protected SizeModifierEditor m_sizeModifierEditor;
		protected SplineUser[] m_users = new SplineUser[0];

		private SerializedProperty m_multithreadedProperty,
			m_updateMethodProperty,
			m_buildOnAwakeProperty,
			m_buildOnEnableProperty,
			m_autoUpdateProperty,
			m_loopSamplesProperty,
			m_clipFromProperty,
			m_clipToProperty;

		protected GUIStyle m_foldoutHeaderStyle;

		private bool m_doRebuild;
		protected SerializedProperty m_spline;

		public int editIndex
		{
			get => m_editIndex;
			set
			{
				if (value == 0)
				{
					Debug.LogError("Cannot set edit index to 0. 0 is reserved.");
					return;
				}

				if (value < -1)
				{
					value = -1;
				}

				m_editIndex = value;
			}
		}

		private int m_editIndex = -1; //0 is reserved for editing clip values

		protected GUIContent m_editButtonContent = new("Edit", "Enable edit mode in scene view");

		protected virtual void HeaderGui()
		{
			var user = (SplineUser)target;
			Undo.RecordObject(user, "Inspector Change");
			var lastSpline = (SplineComputer)m_spline.objectReferenceValue;
			EditorGUILayout.PropertyField(m_spline);
			var newSpline = (SplineComputer)m_spline.objectReferenceValue;
			if (lastSpline != (SplineComputer)m_spline.objectReferenceValue)
			{
				for (var i = 0; i < m_users.Length; i++)
				{
					if (lastSpline != null)
					{
						lastSpline.Unsubscribe(m_users[i]);
					}

					if (newSpline != null)
					{
						newSpline.Subscribe(m_users[i]);
					}
				}

				user.Rebuild();
			}


			if (user.spline == null)
			{
				EditorGUILayout.HelpBox(
					"No SplineComputer is referenced. Link a SplineComputer to make this SplineUser work.",
					MessageType.Error);
			}

			m_settingsFoldout = EditorGUILayout.Foldout(m_settingsFoldout, "User Configuration", m_foldoutHeaderStyle);
			if (m_settingsFoldout)
			{
				EditorGUI.indentLevel++;
				if (m_showClip)
				{
					InspectorClipEdit();
				}

				if (m_showUpdateMethod)
				{
					EditorGUILayout.PropertyField(m_updateMethodProperty);
				}

				EditorGUILayout.PropertyField(m_autoUpdateProperty, new GUIContent("Auto Rebuild"));
				if (m_showMultithreading)
				{
					EditorGUILayout.PropertyField(m_multithreadedProperty);
				}

				EditorGUILayout.PropertyField(m_buildOnAwakeProperty);
				EditorGUILayout.PropertyField(m_buildOnEnableProperty);
				EditorGUI.indentLevel--;
			}
		}

		private void InspectorClipEdit()
		{
			var isClosed = true;
			var loopSamples = true;
			for (var i = 0; i < m_users.Length; i++)
			{
				if (m_users[i].spline == null)
				{
					isClosed = false;
				}
				else if (!m_users[i].spline.isClosed)
				{
					isClosed = false;
				}
				else if (!m_users[i].loopSamples)
				{
					loopSamples = false;
				}
			}

			float clipFrom = m_clipFromProperty.floatValue;
			float clipTo = m_clipToProperty.floatValue;

			if (isClosed && loopSamples)
			{
				EditorGUILayout.BeginHorizontal();
				if (EditButton(m_editIndex == 0))
				{
					if (m_editIndex == 0)
					{
						m_editIndex = -1;
					}
					else
					{
						m_editIndex = 0;
					}
				}

				EditorGUILayout.BeginVertical();
				clipFrom = EditorGUILayout.Slider("Clip From", clipFrom, 0f, 1f);
				clipTo = EditorGUILayout.Slider("Clip To", clipTo, 0f, 1f);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginHorizontal();
				if (EditButton(m_editIndex == 0))
				{
					if (m_editIndex == 0)
					{
						m_editIndex = -1;
					}
					else
					{
						m_editIndex = 0;
					}
				}

				if (GUILayout.Button("Set Distance", GUILayout.Width(85)))
				{
					var w = EditorWindow.GetWindow<ClipRangeWindow>(true);
					var length = 0f;
					if (m_users.Length == 1)
					{
						length = m_users[0].spline.CalculateLength();
					}

					var fromDist = 0f;
					var toDist = 0f;
					var divide = 0;
					for (var i = 0; i < m_users.Length; i++)
					{
						if (m_users[i].spline != null)
						{
							fromDist += m_users[i].spline.CalculateLength(0.0, m_users[i].clipFrom);
							toDist += m_users[i].spline.CalculateLength(0.0, m_users[i].clipTo);
							divide++;
						}
					}

					if (divide > 0)
					{
						fromDist /= divide;
						toDist /= divide;
					}

					w.Init(OnSetClipRangeDistance, fromDist, toDist, length);
				}

				EditorGUIUtility.labelWidth = 80f;
				EditorGUILayout.MinMaxSlider(new GUIContent("Clip Range "), ref clipFrom, ref clipTo, 0f, 1f);
				EditorGUIUtility.labelWidth = 0f;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(30));
				clipFrom = EditorGUILayout.FloatField(clipFrom);
				clipTo = EditorGUILayout.FloatField(clipTo);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndHorizontal();
			}

			m_clipFromProperty.floatValue = clipFrom;
			m_clipToProperty.floatValue = clipTo;
			SplineComputerEditor.hold = m_editIndex >= 0;

			if (isClosed)
			{
				EditorGUILayout.PropertyField(m_loopSamplesProperty, new GUIContent("Loop Samples"));
			}

			if (!m_loopSamplesProperty.boolValue || !isClosed)
			{
				if (m_clipFromProperty.floatValue > m_clipToProperty.floatValue)
				{
					float temp = m_clipToProperty.floatValue;
					m_clipToProperty.floatValue = m_clipFromProperty.floatValue;
					m_clipFromProperty.floatValue = temp;
				}
			}
		}

		private void OnSetClipRangeDistance(float from, float to)
		{
			var longest = 0;
			var max = 0f;
			for (var i = 0; i < m_users.Length; i++)
			{
				if (m_users[i].spline == null)
				{
					continue;
				}

				float length = m_users[i].CalculateLength();
				if (length > max)
				{
					max = length;
					longest = i;
				}
			}

			m_clipFromProperty = serializedObject.FindProperty("_clipFrom");
			m_clipToProperty = serializedObject.FindProperty("_clipTo");
			serializedObject.Update();
			m_clipFromProperty.floatValue = (float)m_users[longest].spline.Travel(0.0, from);
			m_clipToProperty.floatValue = (float)m_users[longest].spline.Travel(0.0, to);

			serializedObject.ApplyModifiedProperties();

			for (var i = 0; i < m_users.Length; i++)
			{
				if (m_users[i].spline == null)
				{
					continue;
				}

				m_users[i].clipFrom = m_clipFromProperty.floatValue;
				m_users[i].clipTo = m_clipToProperty.floatValue;
				m_users[i].RebuildImmediate();
			}
		}

		protected virtual void BodyGui()
		{
			EditorGUILayout.Space();
		}

		protected virtual void FooterGui()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Sample Modifiers", EditorStyles.boldLabel);
			if (m_users.Length == 1)
			{
				if (m_offsetModifierEditor != null)
				{
					m_offsetModifierEditor.DrawInspector();
				}

				if (m_rotationModifierEditor != null)
				{
					m_rotationModifierEditor.DrawInspector();
				}

				if (m_colorModifierEditor != null)
				{
					m_colorModifierEditor.DrawInspector();
				}

				if (m_sizeModifierEditor != null)
				{
					m_sizeModifierEditor.DrawInspector();
				}
			}
			else
			{
				EditorGUILayout.LabelField("Modifiers not available when multiple Spline Users are selected.",
					EditorStyles.centeredGreyMiniLabel);
			}

			EditorGUILayout.Space();
		}

		protected virtual void DuringSceneGui(SceneView currentSceneView)
		{
			if (m_doRebuild)
			{
				DoRebuild();
			}

			var user = (SplineUser)target;
			if (user == null)
			{
				return;
			}

			if (user.spline != null)
			{
				var rootComputer = user.GetComponent<SplineComputer>();
				List<SplineComputer> allComputers = user.spline.GetConnectedComputers();
				for (var i = 0; i < allComputers.Count; i++)
				{
					if (allComputers[i] == rootComputer && m_editIndex == -1)
					{
						continue;
					}

					if (allComputers[i].editorAlwaysDraw)
					{
						continue;
					}

					DssplineDrawer.DrawSplineComputer(allComputers[i], 0.0, 1.0, 0.4f);
				}

				DssplineDrawer.DrawSplineComputer(user.spline);
			}

			if (m_editIndex == 0)
			{
				SceneClipEdit();
			}

			if (m_offsetModifierEditor != null)
			{
				m_offsetModifierEditor.DrawScene();
			}

			if (m_rotationModifierEditor != null)
			{
				m_rotationModifierEditor.DrawScene();
			}

			if (m_colorModifierEditor != null)
			{
				m_colorModifierEditor.DrawScene();
			}

			if (m_sizeModifierEditor != null)
			{
				m_sizeModifierEditor.DrawScene();
			}
		}

		private void SceneClipEdit()
		{
			if (m_users.Length > 1)
			{
				return;
			}

			var user = (SplineUser)target;
			if (user.spline == null)
			{
				return;
			}

			Color col = user.spline.editorPathColor;
			Undo.RecordObject(user, "Edit Clip Range");
			double val = user.clipFrom;
			SplineComputerEditorHandles.Slider(user.spline, ref val, col, "Clip From",
				SplineComputerEditorHandles.SplineSliderGizmo.ForwardTriangle);
			if (val != user.clipFrom)
			{
				user.clipFrom = val;
			}

			val = user.clipTo;
			SplineComputerEditorHandles.Slider(user.spline, ref val, col, "Clip To",
				SplineComputerEditorHandles.SplineSliderGizmo.BackwardTriangle);
			if (val != user.clipTo)
			{
				user.clipTo = val;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (m_doRebuild)
			{
				DoRebuild();
			}

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			HeaderGui();
			if (EditorGUI.EndChangeCheck())
			{
				ApplyAndRebuild();
			}

			EditorGUI.BeginChangeCheck();
			BodyGui();
			if (EditorGUI.EndChangeCheck())
			{
				ApplyAndRebuild();
			}

			EditorGUI.BeginChangeCheck();
			FooterGui();
			if (EditorGUI.EndChangeCheck())
			{
				ApplyAndRebuild();
			}
		}

		private void ApplyAndRebuild()
		{
			serializedObject.ApplyModifiedProperties();
			DoRebuild();
		}

		private void DoRebuild()
		{
			for (var i = 0; i < m_users.Length; i++)
			{
				if (m_users[i] && m_users[i].isActiveAndEnabled)
				{
					try
					{
						m_users[i].Rebuild();
					}
					catch (Exception ex)
					{
						Debug.Log(ex.Message);
					}
				}
			}

			m_doRebuild = false;
		}

		protected virtual void OnDestroy()
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				if (target == null)
				{
					OnDelete(); //The object or the component is being deleted
				}
				else
				{
					DoRebuild();
				}
			}

			SplineComputerEditor.hold = false;

#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= DuringSceneGui;
#endif
		}

		protected virtual void OnDelete()
		{
		}

		protected virtual void Awake()
		{
			m_foldoutHeaderStyle = EditorStyles.foldout;
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += DuringSceneGui;
#endif
			var user = (SplineUser)target;
			user.EditorAwake();
		}

#if !UNITY_2019_1_OR_NEWER
        protected void OnSceneGUI()
        {
            DuringSceneGUI(SceneView.currentDrawingSceneView);
        }
#endif


		protected virtual void OnEnable()
		{
			var user = (SplineUser)target;

			m_settingsFoldout = EditorPrefs.GetBool("Dreamteck.Splines.Editor.SplineUserEditor.settingsFoldout", false);
			m_rotationModifierEditor = new RotationModifierEditor(user, this);
			m_offsetModifierEditor = new OffsetModifierEditor(user, this);
			m_colorModifierEditor = new ColorModifierEditor(user, this);
			m_sizeModifierEditor = new SizeModifierEditor(user, this);

			m_updateMethodProperty = serializedObject.FindProperty("updateMethod");
			m_buildOnAwakeProperty = serializedObject.FindProperty("buildOnAwake");
			m_buildOnEnableProperty = serializedObject.FindProperty("buildOnEnable");
			m_multithreadedProperty = serializedObject.FindProperty("multithreaded");
			m_autoUpdateProperty = serializedObject.FindProperty("_autoUpdate");
			m_loopSamplesProperty = serializedObject.FindProperty("_loopSamples");
			m_clipFromProperty = serializedObject.FindProperty("_clipFrom");
			m_clipToProperty = serializedObject.FindProperty("_clipTo");
			m_spline = serializedObject.FindProperty("_spline");

			m_users = new SplineUser[targets.Length];
			for (var i = 0; i < m_users.Length; i++)
			{
				m_users[i] = (SplineUser)targets[i];
			}

			Undo.undoRedoPerformed += OnUndoRedo;
		}


		protected virtual void OnDisable()
		{
			EditorPrefs.SetBool("Dreamteck.Splines.Editor.SplineUserEditor.settingsFoldout", m_settingsFoldout);
			Undo.undoRedoPerformed -= OnUndoRedo;
		}

		protected virtual void OnUndoRedo()
		{
			m_doRebuild = true;
		}

		public bool EditButton(bool selected)
		{
			var width = 40f;
			m_editButtonContent.image = ResourceUtility.EditorLoadTexture("Splines/Editor/Icons", "edit_cursor");
			if (m_editButtonContent.image != null)
			{
				m_editButtonContent.text = "";
				width = 25f;
			}

			SplineEditorGui.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
			if (SplineEditorGui.EditorLayoutSelectableButton(m_editButtonContent, true, selected,
				    GUILayout.Width(width)))
			{
				SceneView.RepaintAll();
				return true;
			}

			return false;
		}
	}
}
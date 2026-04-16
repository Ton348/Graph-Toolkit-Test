using System;
using System.Collections.Generic;
using Dreamteck.Editor;
using Dreamteck.Splines.Primitives;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class PrimitivesModule : PointTransformModule
	{
		private readonly DreamteckSplinesEditor m_dsEditor;
		private readonly Toolbar m_toolbar;
		private readonly GUIContent[] m_toolbarContents = new GUIContent[2];
		private bool m_createPresetMode;

		private bool m_lastClosed;
		private Spline.Type m_lastType = Spline.Type.Bezier;
		private int m_mode, m_selectedPrimitive, m_selectedPreset;
		private string[] m_presetNames;
		private SplinePreset[] m_presets;
		private PrimitiveEditor[] m_primitiveEditors;
		private string[] m_primitiveNames;

		private string m_savePresetName = "", m_savePresetDescription = "";


		public PrimitivesModule(SplineEditor editor) : base(editor)
		{
			m_dsEditor = (DreamteckSplinesEditor)editor;
			m_toolbarContents[0] = new GUIContent("Primitives", "Procedural Primitives");
			m_toolbarContents[1] = new GUIContent("Presets", "Saved spline presets");
			m_toolbar = new Toolbar(m_toolbarContents, m_toolbarContents);
		}

		public override GUIContent GetIconOff()
		{
			return IconContent("*", "primitives", "Spline Primitives");
		}

		public override GUIContent GetIconOn()
		{
			return IconContent("*", "primitives_on", "Spline Primitives");
		}

		public override void LoadState()
		{
			base.LoadState();
			m_selectedPrimitive = LoadInt("selectedPrimitive");
			m_mode = LoadInt("mode");
			m_createPresetMode = LoadBool("createPresetMode");
		}

		public override void SaveState()
		{
			base.SaveState();
			SaveInt("selectedPrimitive", m_selectedPrimitive);
			SaveInt("mode", m_mode);
			SaveBool("createPresetMode", m_createPresetMode);
		}

		public override void Select()
		{
			base.Select();
			m_lastClosed = m_editor.GetSplineClosed();
			m_lastType = m_editor.GetSplineType();
			if (m_mode == 0)
			{
				LoadPrimitives();
			}
			else if (!m_createPresetMode)
			{
				LoadPresets();
			}
		}

		public override void Deselect()
		{
			ApplyDialog();
			base.Deselect();
		}

		private void ApplyDialog()
		{
			if (!IsDirty())
			{
				return;
			}

			if (EditorUtility.DisplayDialog("Unapplied Primitives",
				    "There is an unapplied primitive. Do you want to apply the changes?", "Apply", "Revert"))
			{
				Apply();
			}
			else
			{
				Revert();
			}
		}

		public override void Revert()
		{
			m_editor.SetSplineType(m_lastType);
			m_editor.SetSplineClosed(m_lastClosed);
			base.Revert();
		}

		protected override void OnDrawInspector()
		{
			EditorGUI.BeginChangeCheck();
			m_toolbar.Draw(ref m_mode);
			if (EditorGUI.EndChangeCheck())
			{
				if (m_mode == 0)
				{
					LoadPrimitives();
				}
				else if (!m_createPresetMode)
				{
					LoadPresets();
				}
			}

			if (selectedPoints.Count > 0)
			{
				ClearSelection();
			}

			if (m_mode == 0)
			{
				PrimitivesGui();
			}
			else
			{
				PresetsGui();
			}

			if (IsDirty() && (!m_createPresetMode || m_mode == 0))
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Apply"))
				{
					Apply();
				}

				if (GUILayout.Button("Revert"))
				{
					Revert();
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		private void PrimitivesGui()
		{
			int last = m_selectedPrimitive;
			m_selectedPrimitive = EditorGUILayout.Popup(m_selectedPrimitive, m_primitiveNames);
			if (last != m_selectedPrimitive)
			{
				m_primitiveEditors[m_selectedPrimitive].Open(m_dsEditor);
				m_primitiveEditors[m_selectedPrimitive].Update();
				TransformPoints();
			}

			EditorGUI.BeginChangeCheck();
			m_primitiveEditors[m_selectedPrimitive].Draw();
			if (EditorGUI.EndChangeCheck())
			{
				TransformPoints();
			}
		}

		private void PresetsGui()
		{
			if (m_createPresetMode)
			{
				m_savePresetName = EditorGUILayout.TextField("Preset name", m_savePresetName);
				EditorGUILayout.LabelField("Description");
				m_savePresetDescription = EditorGUILayout.TextArea(m_savePresetDescription);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					string lower = m_savePresetName.ToLower();
					string noSlashes = lower.Replace('/', '_');
					noSlashes = noSlashes.Replace('\\', '_');
					string noSpaces = noSlashes.Replace(' ', '_');
					var preset = new SplinePreset(points, isClosed, splineType);
					preset.name = m_savePresetName;
					preset.description = m_savePresetDescription;
					preset.Save(noSpaces);
					m_createPresetMode = false;
					LoadPresets();
					m_savePresetName = m_savePresetDescription = "";
				}

				if (GUILayout.Button("Cancel"))
				{
					m_createPresetMode = false;
				}

				EditorGUILayout.EndHorizontal();
				return;
			}

			if (GUILayout.Button("Create New"))
			{
				m_createPresetMode = true;
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			m_selectedPreset =
				EditorGUILayout.Popup(m_selectedPreset, m_presetNames, GUILayout.MaxWidth(Screen.width / 3f));
			if (m_selectedPreset >= 0 && m_selectedPreset < m_presets.Length)
			{
				if (GUILayout.Button("Use"))
				{
					LoadPreset(m_selectedPreset);
				}

				if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
				{
					if (EditorUtility.DisplayDialog("Delete Preset",
						    "This will permanently delete the preset file. Continue?", "Yes", "No"))
					{
						SplinePreset.Delete(m_presets[m_selectedPreset].filename);
						LoadPresets();
						if (m_selectedPreset >= m_presets.Length)
						{
							m_selectedPreset = m_presets.Length - 1;
						}
					}
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private void TransformPoints()
		{
			for (var i = 0; i < m_editor.points.Length; i++)
			{
				m_editor.points[i].position = m_dsEditor.spline.transform.TransformPoint(m_editor.points[i].position);
				m_editor.points[i].tangent = m_dsEditor.spline.transform.TransformPoint(m_editor.points[i].tangent);
				m_editor.points[i].tangent2 = m_dsEditor.spline.transform.TransformPoint(m_editor.points[i].tangent2);
				m_editor.points[i].normal = m_dsEditor.spline.transform.TransformDirection(m_editor.points[i].normal);
			}

			RegisterChange();
			SetDirty();
		}

		private void LoadPrimitives()
		{
			List<Type> types = typeof(PrimitiveEditor).GetAllDerivedClasses();
			m_primitiveEditors = new PrimitiveEditor[types.Count];
			var count = 0;
			m_primitiveNames = new string[types.Count];
			foreach (Type t in types)
			{
				m_primitiveEditors[count] = (PrimitiveEditor)Activator.CreateInstance(t);
				m_primitiveNames[count] = m_primitiveEditors[count].GetName();
				count++;
			}

			if (m_selectedPrimitive >= 0 && m_selectedPrimitive < m_primitiveEditors.Length)
			{
				ClearSelection();
				m_primitiveEditors[m_selectedPrimitive].Open(m_dsEditor);
				m_primitiveEditors[m_selectedPrimitive].Update();
				TransformPoints();
				SetDirty();
			}
		}

		private void LoadPresets()
		{
			ApplyDialog();
			m_presets = SplinePreset.LoadAll();
			m_presetNames = new string[m_presets.Length];
			for (var i = 0; i < m_presets.Length; i++)
			{
				m_presetNames[i] = m_presets[i].name;
			}

			ClearSelection();
		}

		private void LoadPreset(int index)
		{
			if (index >= 0 && index < m_presets.Length)
			{
				m_editor.SetPointsArray(m_presets[index].points);
				m_editor.SetSplineClosed(m_presets[index].isClosed);
				m_editor.SetSplineType(m_presets[index].type);
				TransformPoints();
				FramePoints();
			}
		}
	}
}
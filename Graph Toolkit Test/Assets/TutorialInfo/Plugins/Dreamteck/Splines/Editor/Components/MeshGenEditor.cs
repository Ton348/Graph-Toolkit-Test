using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	[CustomEditor(typeof(MeshGenerator))]
	[CanEditMultipleObjects]
	public class MeshGenEditor : SplineUserEditor
	{
		private BakeMeshWindow m_bakeWindow;
		private bool m_commonFoldout;

		private int m_framesPassed;

		private MeshGenerator[] m_generators = new MeshGenerator[0];
		protected bool m_showColor = true;
		protected bool m_showDoubleSided = true;
		protected bool m_showFlipFaces = true;
		protected bool m_showInfo;
		protected bool m_showNormalMethod = true;
		protected bool m_showOffset = true;
		protected bool m_showRotation = true;
		protected bool m_showSize = true;
		protected bool m_showTangents = true;

		protected override void Awake()
		{
			var generator = (MeshGenerator)target;
			var rend = generator.GetComponent<MeshRenderer>();
			if (rend == null)
			{
				return;
			}

			base.Awake();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			m_generators = new MeshGenerator[targets.Length];
			for (var i = 0; i < targets.Length; i++)
			{
				m_generators[i] = (MeshGenerator)targets[i];
			}

			var user = (MeshGenerator)target;
		}

		protected override void OnDestroy()
		{
			var generator = (MeshGenerator)target;
			base.OnDestroy();
			var gen = (MeshGenerator)target;
			if (gen == null)
			{
				return;
			}

			if (gen.GetComponent<MeshCollider>() != null)
			{
				generator.UpdateCollider();
			}

			if (m_bakeWindow != null)
			{
				m_bakeWindow.Close();
			}
		}

		protected override void DuringSceneGui(SceneView currentSceneView)
		{
			base.DuringSceneGui(currentSceneView);
			var generator = (MeshGenerator)target;
			if (Application.isPlaying)
			{
				return;
			}

			m_framesPassed++;
			if (m_framesPassed >= 100)
			{
				m_framesPassed = 0;
				if (generator != null && generator.GetComponent<MeshCollider>() != null)
				{
					generator.UpdateCollider();
				}
			}
		}

		public override void OnInspectorGUI()
		{
			var generator = (MeshGenerator)target;
			if (generator.baked)
			{
				SplineEditorGui.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
				if (SplineEditorGui.EditorLayoutSelectableButton(
					    new GUIContent("Revert Bake", "Makes the mesh dynamic again and allows editing"), true, true))
				{
					for (var i = 0; i < m_generators.Length; i++)
					{
						m_generators[i].Unbake();
						EditorUtility.SetDirty(m_generators[i]);
					}
				}

				return;
			}

			base.OnInspectorGUI();
		}

		protected override void BodyGui()
		{
			base.BodyGui();
			var generator = (MeshGenerator)target;
			serializedObject.Update();
			SerializedProperty calculateTangents = serializedObject.FindProperty("_calculateTangents");
			SerializedProperty markDynamic = serializedObject.FindProperty("_markDynamic");
			SerializedProperty size = serializedObject.FindProperty("_size");
			SerializedProperty color = serializedObject.FindProperty("_color");
			SerializedProperty normalMethod = serializedObject.FindProperty("_normalMethod");
			SerializedProperty useSplineSize = serializedObject.FindProperty("_useSplineSize");
			SerializedProperty useSplineColor = serializedObject.FindProperty("_useSplineColor");
			SerializedProperty offset = serializedObject.FindProperty("_offset");
			SerializedProperty rotation = serializedObject.FindProperty("_rotation");
			SerializedProperty flipFaces = serializedObject.FindProperty("_flipFaces");
			SerializedProperty doubleSided = serializedObject.FindProperty("_doubleSided");
			SerializedProperty meshIndexFormat = serializedObject.FindProperty("_meshIndexFormat");

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			m_commonFoldout = EditorGUILayout.Foldout(m_commonFoldout, "Common", m_foldoutHeaderStyle);
			if (m_commonFoldout)
			{
				EditorGUI.indentLevel++;
				if (m_showSize)
				{
					EditorGUILayout.PropertyField(size, new GUIContent("Size"));
				}

				if (m_showColor)
				{
					EditorGUILayout.PropertyField(color, new GUIContent("Color"));
				}

				if (m_showNormalMethod)
				{
					EditorGUILayout.PropertyField(normalMethod, new GUIContent("Normal Method"));
				}

				if (m_showOffset)
				{
					EditorGUILayout.PropertyField(offset, new GUIContent("Offset"));
				}

				if (m_showRotation)
				{
					EditorGUILayout.PropertyField(rotation, new GUIContent("Rotation"));
				}

				if (m_showTangents)
				{
					EditorGUILayout.PropertyField(calculateTangents, new GUIContent("Calculate Tangents"));
				}

				EditorGUILayout.PropertyField(useSplineSize, new GUIContent("Use Spline Size"));
				EditorGUILayout.PropertyField(useSplineColor, new GUIContent("Use Spline Color"));
				EditorGUILayout.PropertyField(markDynamic,
					new GUIContent("Mark Dynamic",
						"Improves performance in situations where the mesh changes frequently"));
				EditorGUILayout.PropertyField(meshIndexFormat,
					new GUIContent("Index Format",
						"Format of the mesh index buffer data. Index buffer can either be 16 bit(supports up to 65535 vertices in a mesh), or 32 bit(supports up to 4 billion vertices).Default index format is 16 bit, since that takes less memory and bandwidth."));
				if (meshIndexFormat.enumValueIndex > 0)
				{
					EditorGUILayout.HelpBox(
						"Note that GPU support for 32 bit indices is not guaranteed on all platforms; for example Android devices with Mali-400 GPU do not support them.",
						MessageType.Warning);
				}

				EditorGUI.indentLevel--;
			}

			if (m_showDoubleSided || m_showFlipFaces)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Faces", EditorStyles.boldLabel);
				if (m_showDoubleSided)
				{
					EditorGUILayout.PropertyField(doubleSided, new GUIContent("Double-sided"));
				}

				if (!generator.doubleSided && m_showFlipFaces)
				{
					EditorGUILayout.PropertyField(flipFaces, new GUIContent("Flip Faces"));
				}
			}

			if (generator.GetComponent<MeshCollider>() != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Mesh Collider", EditorStyles.boldLabel);
				generator.colliderUpdateRate =
					EditorGUILayout.FloatField("Collider Update Iterval", generator.colliderUpdateRate);
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				for (var i = 0; i < m_generators.Length; i++)
				{
					m_generators[i].Rebuild();
				}
			}
		}

		protected override void FooterGui()
		{
			base.FooterGui();
			m_showInfo = EditorGUILayout.Foldout(m_showInfo, "Info & Components");
			if (m_showInfo)
			{
				var generator = (MeshGenerator)target;
				var filter = generator.GetComponent<MeshFilter>();
				if (filter == null)
				{
					return;
				}

				var renderer = generator.GetComponent<MeshRenderer>();
				var str = "";
				if (filter.sharedMesh != null)
				{
					str = "Vertices: " + filter.sharedMesh.vertexCount + "\r\nTriangles: " +
					      filter.sharedMesh.triangles.Length / 3;
				}
				else
				{
					str = "No info available";
				}

				EditorGUILayout.HelpBox(str, MessageType.Info);
				bool showFilter = filter.hideFlags == HideFlags.None;
				bool last = showFilter;
				showFilter = EditorGUILayout.Toggle("Show Mesh Filter", showFilter);
				if (last != showFilter)
				{
					if (showFilter)
					{
						filter.hideFlags = HideFlags.None;
					}
					else
					{
						filter.hideFlags = HideFlags.HideInInspector;
					}
				}

				bool showRenderer = renderer.hideFlags == HideFlags.None;
				last = showRenderer;
				showRenderer = EditorGUILayout.Toggle("Show Mesh Renderer", showRenderer);
				if (last != showRenderer)
				{
					if (showRenderer)
					{
						renderer.hideFlags = HideFlags.None;
					}
					else
					{
						renderer.hideFlags = HideFlags.HideInInspector;
					}
				}
			}

			if (m_generators.Length == 1)
			{
				if (GUILayout.Button("Bake Mesh"))
				{
					var generator = (MeshGenerator)target;
					m_bakeWindow = EditorWindow.GetWindow<BakeMeshWindow>();
					m_bakeWindow.Init(generator);
				}
			}
		}

		protected override void OnDelete()
		{
			base.OnDelete();
			var generator = (MeshGenerator)target;
			if (generator == null)
			{
				return;
			}

			var filter = generator.GetComponent<MeshFilter>();
			if (filter != null)
			{
				filter.hideFlags = HideFlags.None;
			}

			var renderer = generator.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				renderer.hideFlags = HideFlags.None;
			}
		}

		protected virtual void Uvcontrols(MeshGenerator generator)
		{
			serializedObject.Update();
			SerializedProperty uvMode = serializedObject.FindProperty("_uvMode");
			SerializedProperty uvOffset = serializedObject.FindProperty("_uvOffset");
			SerializedProperty uvRotation = serializedObject.FindProperty("_uvRotation");
			SerializedProperty uvScale = serializedObject.FindProperty("_uvScale");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(uvMode, new GUIContent("UV Mode"));
			EditorGUILayout.PropertyField(uvOffset, new GUIContent("UV Offset"));
			EditorGUILayout.PropertyField(uvRotation, new GUIContent("UV Rotation"));
			EditorGUILayout.PropertyField(uvScale, new GUIContent("UV Scale"));
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
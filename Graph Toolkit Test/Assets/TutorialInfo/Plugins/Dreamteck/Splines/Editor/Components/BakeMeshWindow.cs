using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class BakeMeshWindow : EditorWindow
	{
		public enum SaveFormat
		{
			MeshAsset,
			OBJ,
			Scene
		}

		public bool isStatic = true;
		public bool copy;
		public bool removeComputer;
		public bool permanent;
		public bool generateLightmapUvs;

		private MeshFilter m_filter;

		private SaveFormat m_format = SaveFormat.MeshAsset;
		private MeshGenerator m_meshGen;
		private MeshRenderer m_renderer;

		private void OnDestroy()
		{
			EditorPrefs.SetBool("BakeWindow_isStatic", isStatic);
			EditorPrefs.SetBool("BakeWindow_generateLightmapUVs", generateLightmapUvs);
			EditorPrefs.SetBool("BakeWindow_copy", copy);
			EditorPrefs.SetBool("BakeWindow_removeComputer", removeComputer);
			EditorPrefs.SetBool("BakeWindow_permanent", permanent);
			EditorPrefs.SetInt("BakeWindow_format", (int)m_format);
		}

		public void Init(MeshGenerator generator)
		{
			titleContent = new GUIContent("Bake Mesh");
			m_meshGen = generator;
			m_filter = generator.GetComponent<MeshFilter>();
			m_renderer = generator.GetComponent<MeshRenderer>();
			if (EditorPrefs.HasKey("BakeWindow_isStatic"))
			{
				isStatic = EditorPrefs.GetBool("BakeWindow_isStatic");
			}

			if (EditorPrefs.HasKey("BakeWindow_generateLightmapUVs"))
			{
				generateLightmapUvs = EditorPrefs.GetBool("BakeWindow_generateLightmapUVs");
			}

			if (EditorPrefs.HasKey("BakeWindow_copy"))
			{
				copy = EditorPrefs.GetBool("BakeWindow_copy");
			}

			if (EditorPrefs.HasKey("BakeWindow_removeComputer"))
			{
				removeComputer = EditorPrefs.GetBool("BakeWindow_removeComputer");
			}

			if (EditorPrefs.HasKey("BakeWindow_permanent"))
			{
				permanent = EditorPrefs.GetBool("BakeWindow_permanent");
			}

			m_format = (SaveFormat)EditorPrefs.GetInt("BakeWindow_format", 0);
			minSize = new Vector2(340, 220);
			maxSize = minSize;
		}

		private void OnGui()
		{
			m_format = (SaveFormat)EditorGUILayout.EnumPopup("Save Format", m_format);
			bool saveMesh = m_format != SaveFormat.Scene;

			if (m_format != SaveFormat.Scene)
			{
				copy = EditorGUILayout.Toggle("Save without baking", copy);
			}

			bool isCopy = m_format != SaveFormat.Scene && copy;
			switch (m_format)
			{
				case SaveFormat.Scene:
					EditorGUILayout.HelpBox("Saves the mesh inside the scene for lightmap", MessageType.Info); break;
				case SaveFormat.MeshAsset:
					EditorGUILayout.HelpBox(
						"Saves the mesh as an .asset file inside the project. This makes using the mesh in prefabs and across scenes possible.",
						MessageType.Info); break;
				case SaveFormat.OBJ:
					EditorGUILayout.HelpBox(
						"Exports the mesh as an OBJ file which can be imported in a third-party modeling application.",
						MessageType.Info); break;
			}

			EditorGUILayout.Space();

			if (!isCopy)
			{
				isStatic = EditorGUILayout.Toggle("Make Static", isStatic);
				permanent = EditorGUILayout.Toggle("Permanent", permanent);
				generateLightmapUvs = EditorGUILayout.Toggle("Generate Lightmap UVs", generateLightmapUvs);
				if (permanent)
				{
					removeComputer = EditorGUILayout.Toggle("Remove SplineComputer", removeComputer);
					if (m_meshGen.spline.subscriberCount > 1 && !isCopy)
					{
						EditorGUILayout.HelpBox(
							"WARNING: Removing the SplineComputer from this object will cause other SplineUsers to malfunction!",
							MessageType.Warning);
					}
				}
			}

			var bakeText = "Bake Mesh";
			if (saveMesh)
			{
				bakeText = "Bake & Save Mesh";
			}

			if (isCopy)
			{
				bakeText = "Save Mesh";
			}

			if (GUILayout.Button(bakeText))
			{
				if (permanent)
				{
					if (!EditorUtility.DisplayDialog("Permanent bake?",
						    "This operation will remove the Mesh Generator. Are you sure you want to continue?", "Yes",
						    "No"))
					{
						return;
					}
				}

				var savePath = "";
				if (saveMesh)
				{
					var ext = "asset";
					if (m_format == SaveFormat.OBJ)
					{
						ext = "obj";
					}

					var meshName = "mesh";
					if (m_filter != null)
					{
						meshName = m_filter.sharedMesh.name;
					}

					savePath = EditorUtility.SaveFilePanel("Save " + meshName, Application.dataPath,
						meshName + "." + ext, ext);
					if (!Directory.Exists(Path.GetDirectoryName(savePath)) || savePath == "")
					{
						EditorUtility.DisplayDialog("Save error",
							"Invalid save path. Please select a valid save path and try again", "OK");
						return;
					}

					if (m_format == SaveFormat.OBJ && !copy && !savePath.StartsWith(Application.dataPath))
					{
						EditorUtility.DisplayDialog("Save error",
							"OBJ files can be saved outside of the project folder only when \"Save without baking\" is selected. Please select a directory inside the project in order to save.",
							"OK");
						return;
					}

					if (m_format == SaveFormat.MeshAsset && !savePath.StartsWith(Application.dataPath))
					{
						EditorUtility.DisplayDialog("Save error",
							"Asset files cannot be saved outside of the project directory. Please select a path inside the project directory.",
							"OK");
						return;
					}
				}

				Undo.RecordObject(m_meshGen.gameObject, "Bake mesh");
				if (!isCopy)
				{
					Bake();
				}
				else
				{
					UnityEditor.MeshUtility.Optimize(m_filter.sharedMesh);
					Unwrapping.GenerateSecondaryUVSet(m_filter.sharedMesh);
				}

				if (saveMesh)
				{
					SaveMeshFile(savePath);
				}
			}
		}

		private void Bake()
		{
			m_meshGen.Bake(isStatic, generateLightmapUvs);
			EditorUtility.SetDirty(m_meshGen);
			if (permanent && !copy)
			{
				SplineComputer meshGenComputer = m_meshGen.spline;
				if (permanent)
				{
					meshGenComputer.Unsubscribe(m_meshGen);

					if (removeComputer && m_meshGen.transform.IsChildOf(meshGenComputer.transform))
					{
						DestroyImmediate(meshGenComputer);
					}

					DestroyImmediate(m_meshGen);
				}

				if (removeComputer && meshGenComputer != null)
				{
					if (meshGenComputer.GetComponents<Component>().Length == 2)
					{
						DestroyImmediate(meshGenComputer.gameObject);
					}
					else
					{
						DestroyImmediate(meshGenComputer);
					}
				}
			}
		}

		private void SaveMeshFile(string savePath)
		{
			if (m_format == SaveFormat.Scene)
			{
				return;
			}

			var relativePath = "";
			if (savePath.StartsWith(Application.dataPath))
			{
				relativePath = "Assets" + savePath.Substring(Application.dataPath.Length);
			}

			if (m_format == SaveFormat.MeshAsset)
			{
				if (copy)
				{
					Mesh assetMesh = MeshUtility.Copy(m_filter.sharedMesh);
					AssetDatabase.CreateAsset(assetMesh, relativePath);
				}
				else
				{
					AssetDatabase.CreateAsset(m_filter.sharedMesh, relativePath);
				}
			}

			if (m_format == SaveFormat.OBJ)
			{
				string objString = MeshUtility.ToObjstring(m_filter.sharedMesh, m_renderer.sharedMaterials);
				File.WriteAllText(savePath, objString);
				if (!copy)
				{
					DestroyImmediate(m_filter.sharedMesh);
				}

				if (relativePath != "") //Import back the OBJ
				{
					AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceSynchronousImport);
					if (!copy)
					{
						m_filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(relativePath);
					}
				}
			}
		}
	}
}
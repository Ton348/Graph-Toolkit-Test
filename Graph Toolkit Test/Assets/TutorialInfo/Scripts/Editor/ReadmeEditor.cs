using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TutorialInfo.Scripts.Editor
{
	[CustomEditor(typeof(Readme))]
	[InitializeOnLoad]
	public class ReadmeEditor : UnityEditor.Editor
	{
		private const float s_kSpace = 16f;
		private static readonly string s_showedReadmeSessionStateName = "ReadmeEditor.showedReadme";

		private static readonly string s_readmeSourceDirectory = "Assets/TutorialInfo";

		[SerializeField]
		private GUIStyle m_linkStyle;

		[SerializeField]
		private GUIStyle m_titleStyle;

		[SerializeField]
		private GUIStyle m_headingStyle;

		[SerializeField]
		private GUIStyle m_bodyStyle;

		[SerializeField]
		private GUIStyle m_buttonStyle;

		private bool m_initialized;

		static ReadmeEditor()
		{
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		private GUIStyle linkStyle => m_linkStyle;

		private GUIStyle titleStyle => m_titleStyle;

		private GUIStyle headingStyle => m_headingStyle;

		private GUIStyle bodyStyle => m_bodyStyle;

		private GUIStyle buttonStyle => m_buttonStyle;

		private static void RemoveTutorial()
		{
			if (EditorUtility.DisplayDialog("Remove Readme Assets",
				    $"All contents under {s_readmeSourceDirectory} will be removed, are you sure you want to proceed?",
				    "Proceed",
				    "Cancel"))
			{
				if (Directory.Exists(s_readmeSourceDirectory))
				{
					FileUtil.DeleteFileOrDirectory(s_readmeSourceDirectory);
					FileUtil.DeleteFileOrDirectory(s_readmeSourceDirectory + ".meta");
				}

				Readme readmeAsset = SelectReadme();
				if (readmeAsset != null)
				{
					string path = AssetDatabase.GetAssetPath(readmeAsset);
					FileUtil.DeleteFileOrDirectory(path + ".meta");
					FileUtil.DeleteFileOrDirectory(path);
				}

				AssetDatabase.Refresh();
			}
		}

		private static void SelectReadmeAutomatically()
		{
			if (!SessionState.GetBool(s_showedReadmeSessionStateName, false))
			{
				Readme readme = SelectReadme();
				SessionState.SetBool(s_showedReadmeSessionStateName, true);

				if (readme && !readme.loadedLayout)
				{
					LoadLayout();
					readme.loadedLayout = true;
				}
			}
		}

		private static void LoadLayout()
		{
			Assembly assembly = typeof(EditorApplication).Assembly;
			Type windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
			MethodInfo method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
			method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
		}

		private static Readme SelectReadme()
		{
			string[] ids = AssetDatabase.FindAssets("Readme t:Readme");
			if (ids.Length == 1)
			{
				Object readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

				Selection.objects = new[] { readmeObject };

				return (Readme)readmeObject;
			}

			return null;
		}

		protected override void OnHeaderGUI()
		{
			var readme = (Readme)target;
			Init();

			float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				if (readme.icon != null)
				{
					GUILayout.Space(s_kSpace);
					GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				}

				GUILayout.Space(s_kSpace);
				GUILayout.BeginVertical();
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label(readme.title, titleStyle);
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			var readme = (Readme)target;
			Init();

			foreach (Readme.Section section in readme.sections)
			{
				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label(section.heading, headingStyle);
				}

				if (!string.IsNullOrEmpty(section.text))
				{
					GUILayout.Label(section.text, bodyStyle);
				}

				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}

				GUILayout.Space(s_kSpace);
			}

			if (GUILayout.Button("Remove Readme Assets", buttonStyle))
			{
				RemoveTutorial();
			}
		}

		private void Init()
		{
			if (m_initialized)
			{
				return;
			}

			m_bodyStyle = new GUIStyle(EditorStyles.label);
			m_bodyStyle.wordWrap = true;
			m_bodyStyle.fontSize = 14;
			m_bodyStyle.richText = true;

			m_titleStyle = new GUIStyle(m_bodyStyle);
			m_titleStyle.fontSize = 26;

			m_headingStyle = new GUIStyle(m_bodyStyle);
			m_headingStyle.fontStyle = FontStyle.Bold;
			m_headingStyle.fontSize = 18;

			m_linkStyle = new GUIStyle(m_bodyStyle);
			m_linkStyle.wordWrap = false;

			// Match selection color which works nicely for both light and dark skins
			m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_linkStyle.stretchWidth = false;

			m_buttonStyle = new GUIStyle(EditorStyles.miniButton);
			m_buttonStyle.fontStyle = FontStyle.Bold;

			m_initialized = true;
		}

		private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			Rect position = GUILayoutUtility.GetRect(label, linkStyle, options);

			Handles.BeginGUI();
			Handles.color = linkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, linkStyle);
		}
	}
}
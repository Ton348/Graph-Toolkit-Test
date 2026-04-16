using System;
using System.Collections.Generic;
using System.IO;
using Dreamteck.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Dreamteck.Splines.Editor
{
	[InitializeOnLoad]
	public static class PluginInfo
	{
		public static string version = "3.0.6";
		private static bool s_open;

		static PluginInfo()
		{
			if (s_open)
			{
				return;
			}

			bool showInfo = EditorPrefs.GetString("Dreamteck.Splines.Info.version", "") != version;

			if (!showInfo)
			{
				var url = "https://dreamteck.io/plugins/splines/welcome.json";
				var prefKey = "Dreamteck.Splines.welcomeScreenVersion";
				int welcomeScreenVersion = EditorPrefs.GetInt(prefKey, -1);

				using (UnityWebRequest mainDataReq = UnityWebRequest.Get(url))
				{
					mainDataReq.SendWebRequest();

					while (!mainDataReq.isDone || mainDataReq.result == UnityWebRequest.Result.InProgress)
					{
					}

					if (mainDataReq.result == UnityWebRequest.Result.ProtocolError ||
					    mainDataReq.result == UnityWebRequest.Result.DataProcessingError ||
					    mainDataReq.result == UnityWebRequest.Result.ConnectionError)
					{
						Debug.LogError("An error occured while fetching the banners data.");
					}
					else if (!showInfo)
					{
						var jObj = JsonUtility.FromJson<WelcomeWindow.Data>(mainDataReq.downloadHandler.text);
						welcomeScreenVersion = jObj.version;

						int currentVersion = EditorPrefs.GetInt(prefKey, -1);

						showInfo = currentVersion < welcomeScreenVersion;
					}
				}
			}

			if (!showInfo)
			{
				return;
			}

			EditorPrefs.SetString("Dreamteck.Splines.Info.version", version);
			EditorApplication.update += OpenWindowOnUpdate;
		}

		private static void OpenWindowOnUpdate()
		{
			EditorApplication.update -= OpenWindowOnUpdate;
			EditorWindow.GetWindow<WelcomeScreen>(true);
			s_open = true;
		}
	}

	[InitializeOnLoad]
	public static class AddScriptingDefines
	{
		static AddScriptingDefines()
		{
			ScriptingDefineUtility.Add("DREAMTECK_SPLINES", EditorUserBuildSettings.selectedBuildTargetGroup, true);
		}
	}

	public class WelcomeScreen : WelcomeWindow
	{
		private ModuleInstaller m_examplesInstaller;
		private ModuleInstaller m_playmakerInstaller;
		private ModuleInstaller m_tmproInstaller;
		protected override Vector2 windowSize => new(450, 620);

		[MenuItem("Window/Dreamteck/Splines/Start Screen")]
		public static void OpenWindow()
		{
			GetWindow<WelcomeScreen>(true);
		}

		protected override void GetHeader()
		{
			m_header = ResourceUtility.EditorLoadTexture("Splines/Editor/Icons", "plugin_header");
		}

		public override void Load()
		{
			base.Load();

			SetTitle("Dreamteck Splines " + PluginInfo.version, "");
			m_panels = new WindowPanel[7];
			m_panels[0] = new WindowPanel("Home", true, 0.25f);
			m_panels[1] = new WindowPanel("Changelog", false, m_panels[0], 0.25f);
			m_panels[2] = new WindowPanel("Learn", false, m_panels[0], 0.25f);
			m_panels[3] = new WindowPanel("Support", false, m_panels[0], 0.25f);
			m_panels[4] = new WindowPanel("Examples", false, m_panels[2], 0.25f);
			m_panels[5] = new WindowPanel("Playmaker", false, m_panels[0], 0.25f);
			m_panels[6] = new WindowPanel("Text Mesh Pro", false, m_panels[0], 0.25f);

			m_panels[0].elements.Add(new WindowPanel.Space(400, 10));
			m_panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "changelog", "What's new?",
				"See all new features, important changes and bugfixes in " + PluginInfo.version,
				new ActionLink(m_panels[1], m_panels[0])));
			m_panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "get_started",
				"Get Started + Packages", "Learn how to use Dreamteck Splines and install core packages",
				new ActionLink(m_panels[2], m_panels[0])));
			m_panels[0].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "support", "Community",
				"Got a problem or a feature request? Join the community!", new ActionLink(m_panels[3], m_panels[0])));

			m_bannerData = LoadBannersData("https://dreamteck.io/plugins/splines/welcome.json",
				"Dreamteck.Splines.welcomeScreenVersion");

			if (m_bannerData != null)
			{
				m_textureWebRequests = new List<UnityWebRequest>();

				for (var i = 0; i < m_bannerData.banners.Length; i++)
				{
					UnityWebRequest request = UnityWebRequestTexture.GetTexture(m_bannerData.banners[i].bannerUrl);
					request.SendWebRequest();
					m_textureWebRequests.Add(request);
					m_hasSentImageRequest = true;
				}

				if (m_hasSentImageRequest)
				{
					EditorApplication.update -= OnEditorUpdate;
					EditorApplication.update += OnEditorUpdate;
				}
			}
			else
			{
				DrawFooter();
			}

			string path = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines/Editor");
			var changelogText = "Changelog file not found.";
			if (Directory.Exists(path))
			{
				if (File.Exists(path + "/changelog.txt"))
				{
					string[] lines = File.ReadAllLines(path + "/changelog.txt");
					changelogText = "";
					for (var i = 0; i < lines.Length; i++)
					{
						changelogText += lines[i] + "\r\n";
					}
				}
			}

			m_panels[1].elements.Add(new WindowPanel.Space(400, 20));
			m_panels[1].elements.Add(new WindowPanel.ScrollText(400, 500, changelogText));

			m_panels[2].elements.Add(new WindowPanel.Space(400, 10));
			m_panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "manual", "User Manual",
				"Read a thorough documentation of the whole package along with a list of API methods.",
				new ActionLink("https://dreamteck-splines.netlify.app/")));
			m_panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "tutorials",
				"Video Tutorials", "Watch a series of Youtube videos to get started.",
				new ActionLink("https://www.youtube.com/playlist?list=PLkZqalQdFIQ6zym8RwSWWl3PZJuUdvNK6")));
			m_panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "examples", "Examples",
				"Install example scenes", new ActionLink(m_panels[4], m_panels[2])));

			m_panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "playmaker",
				"Playmaker Actions", "Install Playmaker actions for Dreamteck Splines",
				new ActionLink(m_panels[5], m_panels[2])));
			m_panels[2].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "tmpro",
				"Text Mesh Pro Support", "Manage components for working with Text Mesh Pro",
				new ActionLink(m_panels[6], m_panels[2])));

			m_panels[3].elements.Add(new WindowPanel.Space(400, 10));
			m_panels[3].elements.Add(new WindowPanel.Thumbnail("Utilities/Editor/Images", "discord", "Discord Server",
				"Join our Discord community and chat with other developers who use Splines.",
				new ActionLink("https://discord.gg/bkYDq8v")));

			m_panels[4].elements.Add(new WindowPanel.Space(400, 10));
			m_panels[4].elements
				.Add(new WindowPanel.Button(400, 30, "Install Examples", new ActionLink(InstallExamples)));
			m_panels[4].elements
				.Add(new WindowPanel.Button(400, 30, "Uninstall Examples", new ActionLink(UnInstallExamples)));

			m_panels[5].elements.Add(new WindowPanel.Space(400, 10));

			m_panels[6].elements
				.Add(new WindowPanel.Button(400, 30, "Install TMPro Support", new ActionLink(InstallTmpro)));
			m_panels[6].elements
				.Add(new WindowPanel.Button(400, 30, "Uninstall TMPro Support", new ActionLink(UninstallTmpro)));

			m_panels[5].elements
				.Add(new WindowPanel.Button(400, 30, "Install Actions", new ActionLink(InstallPlaymaker)));
			m_panels[5].elements
				.Add(new WindowPanel.Button(400, 30, "Uninstall Actions", new ActionLink(UninstallPlaymaker)));

			m_playmakerInstaller = new ModuleInstaller("Splines", "PlaymakerActions");
			m_playmakerInstaller.AddUninstallDirectory("Splines/PlaymakerActions");

			m_examplesInstaller = new ModuleInstaller("Splines", "Examples");
			m_examplesInstaller.AddUninstallDirectory("Splines/Examples");

			m_tmproInstaller = new ModuleInstaller("Splines", "TMPro");
			m_tmproInstaller.AddAssemblyLink("Splines", "Dreamteck.Splines", "Unity.TextMeshPro");
			m_tmproInstaller.AddScriptingDefine("DREAMTECK_SPLINES_TMPRO");
			m_tmproInstaller.AddUninstallDirectory("Splines/Components/TMPro");
			m_tmproInstaller.AddUninstallDirectory("Splines/Editor/Components/TMPro");
		}

		protected override void DrawFooter()
		{
			m_panels[0].elements.Add(new WindowPanel.Space(400, 10));
			m_panels[0].elements
				.Add(new WindowPanel.Label(
					"This window will not appear again automatically. To open it manually go to Window/Dreamteck/Splines/Start Screen",
					s_wrapText, new Color(1f, 1f, 1f, 0.5f), 400, 50));
		}

		private void InstallExamples()
		{
			m_examplesInstaller.Install();
			m_panels[5].Back();
		}

		private void UnInstallExamples()
		{
			m_examplesInstaller.Uninstall();
			m_panels[5].Back();
		}

		private void InstallTmpro()
		{
			m_tmproInstaller.Install();
			m_panels[6].Back();
		}

		private void UninstallTmpro()
		{
			m_tmproInstaller.Uninstall();
			m_panels[6].Back();
		}

		private void InstallPlaymaker()
		{
			m_playmakerInstaller.Install();
			m_panels[5].Back();
		}

		private void UninstallPlaymaker()
		{
			m_playmakerInstaller.Uninstall();
			m_panels[5].Back();
		}

		private static void AddAssemblyReference(string dreamteckAssemblyName, string addedAssemblyName)
		{
			string localDir = ResourceUtility.FindFolder(Application.dataPath, "Dreamteck/Splines");
			string path = Path.Combine(Application.dataPath, localDir, dreamteckAssemblyName + ".asmdef");
			var data = "";
			using (var reader = new StreamReader(path))
			{
				data = reader.ReadToEnd();
			}

			var asmDef = AssemblyDefinition.CreateFromJson(data);
			foreach (string reference in asmDef.references)
			{
				if (reference == addedAssemblyName)
				{
					return;
				}
			}

			UnityEditor.ArrayUtility.Add(ref asmDef.references, addedAssemblyName);
			using (var writer = new StreamWriter(path, false))
			{
				writer.Write(asmDef.ToString());
			}
		}
	}

	[Serializable]
	public struct AssemblyDefinition
	{
		public string name;
		public string rootNamespace;
		public string[] references;
		public string[] includePlatforms;
		public string[] exludePlatforms;
		public bool allowUnsafeCode;
		public bool overrideReferences;
		public string precompiledReferences;
		public bool autoReferenced;
		public string[] defineConstraints;
		public string[] versionDefines;
		public bool noEngineReferences;

		public static AssemblyDefinition CreateFromJson(string json)
		{
			return JsonUtility.FromJson<AssemblyDefinition>(json);
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, true);
		}
	}
}
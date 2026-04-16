using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck
{
	public static class ResourceUtility
	{
		private static readonly string s_dreamteckFolder;
		private static readonly string s_dreamteckLocalFolder;
		private static readonly bool s_directoryIsValid;

		static ResourceUtility()
		{
			string defaultPath = Application.dataPath + "/Dreamteck";
			s_dreamteckFolder = EditorPrefs.GetString("Dreamteck.ResourceUtility.dreamteckProjectFolder", defaultPath);
			if (!s_dreamteckFolder.StartsWith(Application.dataPath))
			{
				s_dreamteckFolder = defaultPath;
			}

			if (!Directory.Exists(s_dreamteckFolder))
			{
				s_dreamteckFolder = FindFolder(Application.dataPath, "Dreamteck");
				s_directoryIsValid = Directory.Exists(s_dreamteckFolder);
			}
			else
			{
				s_directoryIsValid = true;
			}

			if (s_directoryIsValid)
			{
				s_dreamteckLocalFolder = s_dreamteckFolder.Substring(Application.dataPath.Length + 1);
				EditorPrefs.SetString("Dreamteck.ResourceUtility.dreamteckProjectFolder", s_dreamteckFolder);
			}
		}

		//Attempts to find the input directory pattern inside a given directory and if it fails, proceeds with looking up all subfolders
		public static string FindFolder(string dir, string folderPattern)
		{
			if (folderPattern.StartsWith("/"))
			{
				folderPattern = folderPattern.Substring(1);
			}

			if (!dir.EndsWith("/"))
			{
				dir += "/";
			}

			if (folderPattern == "")
			{
				return "";
			}

			string[] folders = folderPattern.Split('/');
			if (folders.Length == 0)
			{
				return "";
			}

			var foundDir = "";
			try
			{
				foreach (string d in Directory.GetDirectories(dir))
				{
					var dirInfo = new DirectoryInfo(d);
					if (dirInfo.Name == folders[0])
					{
						foundDir = d;
						string searchDir = FindFolder(d, string.Join("/", folders, 1, folders.Length - 1));
						if (searchDir != "")
						{
							foundDir = searchDir;
							break;
						}
					}
				}

				if (foundDir == "")
				{
					foreach (string d in Directory.GetDirectories(dir))
					{
						foundDir = FindFolder(d, string.Join("/", folders));
						if (foundDir != "")
						{
							break;
						}
					}
				}
			}
			catch (Exception excpt)
			{
				Debug.LogError(excpt.Message);
				return "";
			}

			return foundDir;
		}

		public static Texture2D LoadTexture(string dreamteckPath, string textureFileName)
		{
			string path = Application.dataPath + "/Dreamteck/" + dreamteckPath;
			if (!Directory.Exists(path))
			{
				path = FindFolder(Application.dataPath, "Dreamteck/" + dreamteckPath);
				if (!Directory.Exists(path))
				{
					return null;
				}
			}

			if (!File.Exists(path + "/" + textureFileName))
			{
				return null;
			}

			byte[] bytes = File.ReadAllBytes(path + "/" + textureFileName);
			var result = new Texture2D(1, 1);
			result.name = textureFileName;
			result.LoadImage(bytes);
			return result;
		}

		public static Texture2D LoadTexture(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

			byte[] bytes = File.ReadAllBytes(path);
			var result = new Texture2D(1, 1);
			var finfo = new FileInfo(path);
			result.name = finfo.Name;
			result.LoadImage(bytes);
			return result;
		}

		public static Texture2D[] EditorLoadTextures(string dreamteckLocalPath)
		{
			string path = "Assets/" + s_dreamteckLocalFolder + "/" + dreamteckLocalPath;
			string[] textureGUIDs = AssetDatabase.FindAssets("t:texture2d", new[] { path });
			var textures = new Texture2D[textureGUIDs.Length];
			for (var i = 0; i < textureGUIDs.Length; i++)
			{
				textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGUIDs[i]));
			}

			return textures;
		}

		public static Texture2D EditorLoadTexture(string dreamteckLocalPath, string textureName)
		{
			string path = "Assets/" + s_dreamteckLocalFolder + "/" + dreamteckLocalPath + "/" + textureName + ".png";
			var texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
			return texture;
		}
	}
}
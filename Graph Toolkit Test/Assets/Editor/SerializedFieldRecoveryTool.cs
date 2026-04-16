using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class SerializedFieldRecoveryTool
{
	private static readonly string[] AssetExtensions =
	{
		".prefab",
		".unity",
		".asset",
	};

	private const string MenuRoot = "Tools/Serialization/";

	[MenuItem(MenuRoot + "Recover Renamed Serialized Fields")]
	public static void RecoverRenamedSerializedFields()
	{
		if (EditorSettings.serializationMode != SerializationMode.ForceText)
		{
			var result = EditorUtility.DisplayDialog(
				"Serialization Mode is not Force Text",
				"Эта тулза работает по YAML-тексту ассетов. " +
				"Сейчас Project Settings > Editor > Asset Serialization Mode не Force Text.\n\n" +
				"Продолжить все равно?",
				"Continue",
				"Cancel");

			if (!result)
			{
				return;
			}
		}

		try
		{
			var scriptInfos = BuildScriptInfos();
			if (scriptInfos.Count == 0)
			{
				Debug.LogWarning("No eligible MonoBehaviour / ScriptableObject scripts found.");
				return;
			}

			var assetPaths = AssetDatabase.GetAllAssetPaths()
				.Where(p => p.StartsWith("Assets/", StringComparison.Ordinal))
				.Where(p => AssetExtensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
				.Where(File.Exists)
				.ToArray();

			int changedFiles = 0;
			int totalReplacements = 0;

			AssetDatabase.StartAssetEditing();
			try
			{
				for (int i = 0; i < assetPaths.Length; i++)
				{
					var assetPath = assetPaths[i];
					EditorUtility.DisplayProgressBar(
						"Recover Renamed Serialized Fields",
						$"{i + 1}/{assetPaths.Length}\n{assetPath}",
						(float)(i + 1) / assetPaths.Length);

					if (ProcessAsset(assetPath, scriptInfos, out int replacementsInFile))
					{
						changedFiles++;
						totalReplacements += replacementsInFile;
					}
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				EditorUtility.ClearProgressBar();
			}

			AssetDatabase.Refresh();

			Debug.Log(
				$"Serialized field recovery complete. " +
				$"Changed files: {changedFiles}, total replacements: {totalReplacements}");
		}
		catch (Exception ex)
		{
			EditorUtility.ClearProgressBar();
			Debug.LogException(ex);
		}
	}

	[MenuItem(MenuRoot + "Preview Renamed Serialized Fields Candidates")]
	public static void PreviewRenamedSerializedFieldsCandidates()
	{
		var scriptInfos = BuildScriptInfos();

		foreach (var kvp in scriptInfos.OrderBy(x => x.Value.Type.FullName))
		{
			var info = kvp.Value;
			Debug.Log($"[{info.Type.FullName}] GUID={info.ScriptGuid}");

			foreach (var field in info.Fields.OrderBy(f => f.NewName))
			{
				Debug.Log(
					$"  New: {field.NewName} | Old candidates: {string.Join(", ", field.OldNames.OrderBy(x => x))}");
			}
		}
	}

	private static Dictionary<string, ScriptInfo> BuildScriptInfos()
	{
		var result = new Dictionary<string, ScriptInfo>(StringComparer.Ordinal);

		var monoScriptGuids = AssetDatabase.FindAssets("t:MonoScript");
		foreach (var guid in monoScriptGuids)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			if (script == null)
			{
				continue;
			}

			var type = script.GetClass();
			if (type == null)
			{
				continue;
			}

			if (!typeof(MonoBehaviour).IsAssignableFrom(type) &&
			    !typeof(ScriptableObject).IsAssignableFrom(type))
			{
				continue;
			}

			var fields = GetSerializedFields(type)
				.Select(BuildFieldRule)
				.Where(r => r.OldNames.Count > 0)
				.ToList();

			if (fields.Count == 0)
			{
				continue;
			}

			result[guid] = new ScriptInfo
			{
				ScriptGuid = guid,
				Type = type,
				Fields = fields,
			};
		}

		return result;
	}

	private static IEnumerable<FieldInfo> GetSerializedFields(Type type)
	{
		const BindingFlags flags =
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.DeclaredOnly;

		var current = type;
		while (current != null && current != typeof(MonoBehaviour) && current != typeof(ScriptableObject))
		{
			foreach (var field in current.GetFields(flags))
			{
				if (field.IsStatic)
				{
					continue;
				}

				if (field.IsNotSerialized)
				{
					continue;
				}

				if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
				{
					continue;
				}

				bool isPublic = field.IsPublic;
				bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
				bool hasSerializeReference = field.GetCustomAttribute<SerializeReference>() != null;

				if (!isPublic && !hasSerializeField && !hasSerializeReference)
				{
					continue;
				}

				yield return field;
			}

			current = current.BaseType;
		}
	}

	private static FieldRule BuildFieldRule(FieldInfo field)
	{
		var newName = field.Name;
		var oldNames = BuildOldNameCandidates(newName);

		return new FieldRule
		{
			NewName = newName,
			OldNames = oldNames,
		};
	}

	private static HashSet<string> BuildOldNameCandidates(string newName)
	{
		var set = new HashSet<string>(StringComparer.Ordinal);

		if (string.IsNullOrEmpty(newName))
		{
			return set;
		}

		// m_value -> _value / value / Value
		if (newName.StartsWith("m_", StringComparison.Ordinal) && newName.Length > 2)
		{
			var tail = newName.Substring(2); // value
			AddIfValid(set, "_" + tail);
			AddIfValid(set, tail);
			AddIfValid(set, UpperFirstAscii(tail));
		}

		// mValue -> _value / value / Value / _Value
		if (newName.StartsWith("m", StringComparison.Ordinal) &&
		    newName.Length > 1 &&
		    char.IsUpper(newName[1]))
		{
			var tailPascal = newName.Substring(1); // Value
			var tailCamel = LowerFirstAscii(tailPascal); // value

			AddIfValid(set, tailPascal);
			AddIfValid(set, tailCamel);
			AddIfValid(set, "_" + tailCamel);
			AddIfValid(set, "_" + tailPascal);
		}

		// value -> Value
		if (char.IsLower(newName[0]))
		{
			AddIfValid(set, UpperFirstAscii(newName));
		}

		// _value -> value / Value
		if (newName.StartsWith("_", StringComparison.Ordinal) && newName.Length > 1)
		{
			var tail = newName.Substring(1);
			AddIfValid(set, tail);
			AddIfValid(set, UpperFirstAscii(tail));
		}

		// Убираем совпадение с самим новым именем
		set.Remove(newName);

		return set;
	}

	private static bool ProcessAsset(
		string assetPath,
		Dictionary<string, ScriptInfo> scriptInfos,
		out int replacementsInFile)
	{
		replacementsInFile = 0;

		var text = File.ReadAllText(assetPath);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}

		// Разбиваем на YAML objects: начиная с --- !
		var objectRegex = new Regex(
			@"(?ms)^--- !u![^\r\n]*\r?\n.*?(?=^--- !u!|\z)",
			RegexOptions.Multiline);

		var matches = objectRegex.Matches(text);
		if (matches.Count == 0)
		{
			return false;
		}

		bool changed = false;
		var rewrittenObjects = new List<(int index, int length, string value)>();

		foreach (Match match in matches)
		{
			var block = match.Value;

			if (!TryExtractScriptGuid(block, out var scriptGuid))
			{
				continue;
			}

			if (!scriptInfos.TryGetValue(scriptGuid, out var scriptInfo))
			{
				continue;
			}

			var updatedBlock = block;
			int before = replacementsInFile;

			foreach (var field in scriptInfo.Fields)
			{
				foreach (var oldName in field.OldNames)
				{
					replacementsInFile += ReplaceSerializedKey(ref updatedBlock, oldName, field.NewName);
				}
			}

			if (!ReferenceEquals(updatedBlock, block) && replacementsInFile > before)
			{
				rewrittenObjects.Add((match.Index, match.Length, updatedBlock));
				changed = true;
			}
		}

		if (!changed)
		{
			return false;
		}

		// Применяем замены с конца, чтобы индексы не поплыли
		var finalText = text;
		for (int i = rewrittenObjects.Count - 1; i >= 0; i--)
		{
			var rw = rewrittenObjects[i];
			finalText = finalText.Remove(rw.index, rw.length).Insert(rw.index, rw.value);
		}

		if (finalText == text)
		{
			return false;
		}

		File.WriteAllText(assetPath, finalText);
		return true;
	}

	private static bool TryExtractScriptGuid(string yamlBlock, out string guid)
	{
		// Ищем m_Script: {fileID: ..., guid: XXXXX, type: 3}
		var match = Regex.Match(
			yamlBlock,
			@"m_Script:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-fA-F]{32}),\s*type:\s*\d+\}",
			RegexOptions.Multiline);

		if (match.Success)
		{
			guid = match.Groups[1].Value;
			return true;
		}

		guid = null;
		return false;
	}

	private static int ReplaceSerializedKey(ref string yamlBlock, string oldName, string newName)
	{
		if (oldName == newName)
		{
			return 0;
		}

		// Меняем только ключи YAML вида:
		//   someField: ...
		//
		// Не трогаем строки, где это часть значения.
		// Сохраняем отступ и двоеточие.
		var pattern = $@"(^\s*){Regex.Escape(oldName)}(?=\s*:)";
		var matches = Regex.Matches(yamlBlock, pattern, RegexOptions.Multiline);

		if (matches.Count == 0)
		{
			return 0;
		}

		yamlBlock = Regex.Replace(
			yamlBlock,
			pattern,
			$"$1{newName}",
			RegexOptions.Multiline);

		return matches.Count;
	}

	private static void AddIfValid(HashSet<string> set, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}

		set.Add(value);
	}

	private static string LowerFirstAscii(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}

		if (!char.IsUpper(value[0]))
		{
			return value;
		}

		return char.ToLowerInvariant(value[0]) + value.Substring(1);
	}

	private static string UpperFirstAscii(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}

		if (!char.IsLower(value[0]))
		{
			return value;
		}

		return char.ToUpperInvariant(value[0]) + value.Substring(1);
	}

	private sealed class ScriptInfo
	{
		public string ScriptGuid;
		public Type Type;
		public List<FieldRule> Fields;
	}

	private sealed class FieldRule
	{
		public string NewName;
		public HashSet<string> OldNames;
	}
}
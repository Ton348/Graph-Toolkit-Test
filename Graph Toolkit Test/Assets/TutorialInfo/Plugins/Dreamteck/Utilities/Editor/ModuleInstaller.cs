namespace Dreamteck.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using UnityEditor;

    public class ModuleInstaller
    {
        protected const string s_dReamteckFolderName = "Dreamteck";

        /// <summary>
        /// Local directory within the Dreamteck folder of the unitypackage
        /// </summary>
        private string m_packageDirectory = "";
        private string m_packageName = "";
        private List<string> m_scriptingDefines = new List<string>();
        private List<string> m_uninstallDirectories = new List<string>();
        private Dictionary<string, List<string>> m_assemblyLinks = new Dictionary<string, List<string>>();

        public ModuleInstaller(string packageDirectory, string packageName)
        {
            m_packageDirectory = packageDirectory;
            m_packageName = packageName;
        }

        public void AddAssemblyLink(string dreamteckAssemblyDirectory, string dreamteckAssemblyName, string addedAssemblyName)
        {
            string localFilePath = Path.Combine(s_dReamteckFolderName, dreamteckAssemblyDirectory, dreamteckAssemblyName + ".asmdef");
            if (m_assemblyLinks.ContainsKey(localFilePath))
            {
                m_assemblyLinks[localFilePath].Add(addedAssemblyName);
            } else
            {
                m_assemblyLinks.Add(localFilePath, new List<string>(new string[] { addedAssemblyName }));
            }
        }

        public void AddUninstallDirectory(string dreamteckLocalDirectory)
        {
            if (!m_uninstallDirectories.Contains(dreamteckLocalDirectory))
            {
                m_uninstallDirectories.Add(dreamteckLocalDirectory);
            }
        }

        public void AddScriptingDefine(string define)
        {
            if (!m_scriptingDefines.Contains(define))
            {
                m_scriptingDefines.Add(define);
            }
        }

        public void Install()
        {
            string globalPath = ResourceUtility.FindFolder(Application.dataPath, s_dReamteckFolderName + "/" + m_packageDirectory);
            if (!Directory.Exists(globalPath))
            {
                EditorUtility.DisplayDialog("Missing Package", "Package directory not found: " + m_packageDirectory, "OK");
                return;
            }
            globalPath = Path.Combine(globalPath, m_packageName + ".unitypackage");
            if (!File.Exists(globalPath))
            {
                EditorUtility.DisplayDialog("Missing Package", "Package file not found: " + m_packageDirectory, "OK");
                return;
            }

            foreach (var key in m_assemblyLinks.Keys)
            {
                for (int i = 0; i < m_assemblyLinks[key].Count; i++)
                {
                    AddAssemblyReference(key, m_assemblyLinks[key][i]);
                }
            }

            AssetDatabase.ImportPackage(globalPath, false);
            EditorUtility.DisplayDialog("Import Complete", m_packageName + " is now installed.", "OK");
            for (int i = 0; i < m_scriptingDefines.Count; i++)
            {
                ScriptingDefineUtility.Add(m_scriptingDefines[i], EditorUserBuildSettings.selectedBuildTargetGroup, true);
            }
        }

        public void Uninstall()
        {
            string dialogText = "The assets in the following folders will be removed: \n";
            for (int i = 0; i < m_uninstallDirectories.Count; i++)
            {
                dialogText += m_uninstallDirectories[i] + "\n";
            }
            bool result = EditorUtility.DisplayDialog("Uninstalling", dialogText, "OK", "Cancel");
            if (!result) return;

            for (int i = 0; i < m_uninstallDirectories.Count; i++)
            {
                string globalPath = ResourceUtility.FindFolder(Application.dataPath, s_dReamteckFolderName + "/" + m_uninstallDirectories[i]);
                string relativePath = "Assets" + globalPath.Substring(Application.dataPath.Length);
                Debug.Log("Uninstalling " + relativePath);
                AssetDatabase.DeleteAsset(relativePath);
            }

            foreach (var key in m_assemblyLinks.Keys)
            {
                for (int i = 0; i < m_assemblyLinks[key].Count; i++)
                {
                    RemoveAssemblyReference(key, m_assemblyLinks[key][i]);
                }
            }

            

            for (int i = 0; i < m_scriptingDefines.Count; i++)
            {
                ScriptingDefineUtility.Remove(m_scriptingDefines[i], EditorUserBuildSettings.selectedBuildTargetGroup, true);
            }
        }

        private static void AddAssemblyReference(string dreamteckAssemblyPath, string addedAssemblyName)
        {
            var path = Path.Combine(Application.dataPath, dreamteckAssemblyPath);
            var data = "";
            using (var reader = new StreamReader(path))
            {
                data = reader.ReadToEnd();
            }

            var asmDef = AssemblyDefinition.CreateFromJson(data);
            foreach (var reference in asmDef.references)
            {
                if (reference == addedAssemblyName) return;
            }

            ArrayUtility.Add(ref asmDef.references, addedAssemblyName);
            Debug.Log("Adding " + addedAssemblyName + " to assembly " + dreamteckAssemblyPath);
            using (var writer = new StreamWriter(path, false))
            {
                writer.Write(asmDef.ToString());
            }
        }
        
        private static void RemoveAssemblyReference(string dreamteckAssemblyPath, string addedAssemblyName)
        {
            var path = Path.Combine(Application.dataPath, dreamteckAssemblyPath);
            var data = "";
            using (var reader = new StreamReader(path))
            {
                data = reader.ReadToEnd();
            }

            var asmDef = AssemblyDefinition.CreateFromJson(data);
            bool contains = false;
            foreach (var reference in asmDef.references)
            {
                if (reference != addedAssemblyName) continue;
                contains = true;
                break;
            }
            if (!contains) return;

            ArrayUtility.Remove(ref asmDef.references, addedAssemblyName);
            Debug.Log("Removing " + addedAssemblyName + " from assembly " + dreamteckAssemblyPath);
            using (var writer = new StreamWriter(path, false))
            {
                writer.Write(asmDef.ToString());
            }
        }

        [System.Serializable]
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
}
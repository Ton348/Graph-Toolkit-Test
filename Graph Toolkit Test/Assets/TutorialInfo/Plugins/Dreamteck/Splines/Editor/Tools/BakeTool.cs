namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;

    public class BakeTool : SplineTool
    {
        public enum BakeGroup { All, Selected, AllExcluding }
        BakeGroup m_bakeGroup = BakeGroup.All;
        MeshGenerator[] m_found = new MeshGenerator[0];
        List<MeshGenerator> m_selected = new List<MeshGenerator>();
        List<MeshGenerator> m_excluded = new List<MeshGenerator>();

        bool m_isStatic = true;
        bool m_removeComputer = false;
        bool m_permanent = false;
        bool m_copy = false;
        BakeMeshWindow.SaveFormat m_format = BakeMeshWindow.SaveFormat.MeshAsset;

        string m_savePath = "";

        DirectoryInfo m_dirInfo;

        Vector2 m_scroll1, m_scroll2;

        public override string GetName()
        {
            return "Bake Meshes";
        }

        public override void Draw(Rect windowRect)
        {
            m_bakeGroup = (BakeGroup)EditorGUILayout.EnumPopup("Bake Mode", m_bakeGroup);
            if (m_bakeGroup == BakeGroup.Selected)
            {
                MeshGenSelector(ref m_selected, "Selected");
            } else if(m_bakeGroup == BakeGroup.AllExcluding)
            {
                MeshGenSelector(ref m_excluded, "Excluded");
            }


            m_format = (BakeMeshWindow.SaveFormat)EditorGUILayout.EnumPopup("Save Format", m_format);
            bool saveMesh = m_format != BakeMeshWindow.SaveFormat.Scene;

            if (m_format != BakeMeshWindow.SaveFormat.Scene)
            {
                m_copy = EditorGUILayout.Toggle("Save without baking", m_copy);
            }
            bool isCopy = m_format != BakeMeshWindow.SaveFormat.Scene && m_copy;
            switch (m_format)
            {
                case BakeMeshWindow.SaveFormat.Scene: EditorGUILayout.HelpBox("Saves the mesh inside the scene", MessageType.Info); break;
                case BakeMeshWindow.SaveFormat.MeshAsset: EditorGUILayout.HelpBox("Saves the mesh as an .asset file inside the project. This makes using the mesh in prefabs and across scenes possible.", MessageType.Info); break;
                case BakeMeshWindow.SaveFormat.OBJ: EditorGUILayout.HelpBox("Exports the mesh as an OBJ file which can be imported in a third-party modeling application.", MessageType.Info); break;
            }
            EditorGUILayout.Space();

            if (!isCopy)
            {
                m_isStatic = EditorGUILayout.Toggle("Make Static", m_isStatic);
                m_permanent = EditorGUILayout.Toggle("Permanent", m_permanent);
                if (m_permanent)
                {
                    m_removeComputer = EditorGUILayout.Toggle("Remove SplineComputer", m_removeComputer);
                    if (m_removeComputer) EditorGUILayout.HelpBox("WARNING: Removing Spline Computers may cause other SplineUsers to stop working. Select this if you are sure that no other SplineUser uses the selected Spline Computers.", MessageType.Warning);
                }
            }

            if (GUILayout.Button("Bake"))
            {
                if (saveMesh)
                {
                    m_savePath = EditorUtility.OpenFolderPanel("Save Directory", Application.dataPath, "folder");
                    if (!Directory.Exists(m_savePath) || m_savePath == "")
                    {
                        EditorUtility.DisplayDialog("Save error", "Invalid save directory. Please select a valid save directory and try again", "OK");
                        return;
                    }
                    if (m_format == BakeMeshWindow.SaveFormat.OBJ && !m_savePath.StartsWith(Application.dataPath) && !m_copy)
                    {
                        EditorUtility.DisplayDialog("Save error", "OBJ files can be saved outside of the project folder only when \"Save without baking\" is selected. Please select a directory inside the project in order to save.", "OK");
                        return;
                    }
                    if (m_format == BakeMeshWindow.SaveFormat.MeshAsset && !m_savePath.StartsWith(Application.dataPath))
                    {
                        EditorUtility.DisplayDialog("Save error", "Asset files cannot be saved outside of the project directory. Please select a path inside the project directory.", "OK");
                        return;
                    }
                }
                string suff = "all";
                if (m_bakeGroup == BakeGroup.Selected) suff = "selected";
                if (m_bakeGroup == BakeGroup.AllExcluding) suff = "all excluding";
                if(EditorUtility.DisplayDialog("Bake " + suff, "This operation cannot be undone. Are you sure you want to bake the meshes?", "Yes", "No"))
                {
                    switch (m_bakeGroup)
                    {
                        case BakeGroup.All: BakeAll(); break;
                        case BakeGroup.Selected: BakeSelected(); break;
                        case BakeGroup.AllExcluding: BakeExcluding(); break;
                    }
                }
            }
        }

        private void BakeAll()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < m_found.Length; i++)
            {
                float percent = (float)i / (m_found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(m_found[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void BakeSelected()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < m_selected.Count; i++)
            {
                float percent = (float)i / (m_selected.Count - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(m_selected[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void BakeExcluding()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < m_found.Length; i++)
            {
                float percent = (float)i / (m_found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(m_found[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void Bake(MeshGenerator gen)
        {
            MeshFilter filter = gen.GetComponent<MeshFilter>();
            if(filter == null)
            {
                EditorUtility.DisplayDialog("Save error", "No mesh present in " + gen.name, "OK");
                return;
            }
            if (m_copy)
            {
                UnityEditor.MeshUtility.Optimize(filter.sharedMesh);
               Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
            }
            else gen.Bake(m_isStatic, true);

            if(m_format == BakeMeshWindow.SaveFormat.OBJ)
            {
                MeshRenderer renderer = gen.GetComponent<MeshRenderer>();
                m_dirInfo = new DirectoryInfo(m_savePath);
                FileInfo[] files = m_dirInfo.GetFiles(filter.sharedMesh.name + "*.obj");
                string meshName = filter.sharedMesh.name;
                if (files.Length > 0) meshName += "_" + files.Length;
                string path = m_savePath + "/" + meshName + ".obj";
                string objString = Dreamteck.MeshUtility.ToObjstring(filter.sharedMesh, renderer.sharedMaterials);
                File.WriteAllText(path, objString);
                if (m_copy)
                {
                    string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
                    AssetDatabase.ImportAsset(relativepath, ImportAssetOptions.ForceSynchronousImport);
                    filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(relativepath);
                }
            }

            if(m_format == BakeMeshWindow.SaveFormat.MeshAsset)
            {
                m_dirInfo = new DirectoryInfo(m_savePath);
                FileInfo[] files = m_dirInfo.GetFiles(filter.sharedMesh.name + "*.asset");
                string meshName = filter.sharedMesh.name;
                if (files.Length > 0) meshName += "_" + files.Length;
                string path = m_savePath + "/" + meshName + ".asset";
                string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
                if (m_copy)
                {
                    Mesh assetMesh = Dreamteck.MeshUtility.Copy(filter.sharedMesh);
                    AssetDatabase.CreateAsset(assetMesh, relativepath);
                } else AssetDatabase.CreateAsset(filter.sharedMesh, relativepath);
            }

            if (m_permanent && !m_copy)
            {
                SplineComputer meshGenComputer = gen.spline;
                if (m_permanent)
                {
                    meshGenComputer.Unsubscribe(gen);
                    Object.DestroyImmediate(gen);
                }
                if (m_removeComputer)
                {
                    if (meshGenComputer.GetComponents<Component>().Length == 2) Object.DestroyImmediate(meshGenComputer.gameObject);
                    else Object.DestroyImmediate(meshGenComputer);
                }
            }
        }

        private void Refresh()
        {
            m_found = Object.FindObjectsOfType<MeshGenerator>();
        }

        void OnFocus()
        {
            Refresh();
        }

        public override void Open(EditorWindow window)
        {
            base.Open(window);
            m_isStatic = LoadBool("isStatic", true);
            m_format = (BakeMeshWindow.SaveFormat)LoadInt("format", 0);
            m_removeComputer = LoadBool("removeComputer", false);
            m_copy = LoadBool("copy", false);
            Refresh();
        }

        public override void Close()
        {
            base.Close();
            SaveBool("isStatic", m_isStatic);
            SaveInt("format", (int)m_format);
            SaveBool("copy", m_copy);
            SaveBool("removeComputer", m_removeComputer);
        }

        protected override string GetPrefix()
        {
            return "BakeTool";
        }

        private void MeshGenSelector(ref List<MeshGenerator> list, string title)
        {
            List<MeshGenerator> availalbe = new List<MeshGenerator>(m_found);
            for (int i = availalbe.Count-1; i >= 0; i--)
            {
                for (int n = 0; n < list.Count; n++)
                {
                    if (list[n] == availalbe[i])
                    {
                        availalbe.RemoveAt(i);
                        break;
                    }
                }
            }
            GUILayout.Box("Available", GUILayout.Width(Screen.width - 15 - Screen.width/3f), GUILayout.Height(100));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += 15;
            rect.height -= 15;
            m_scroll1 = GUI.BeginScrollView(rect, m_scroll1, new Rect(0, 0, rect.width, 22 * availalbe.Count));
            for (int i = 0; i < availalbe.Count; i++)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), availalbe[i].name);
                if (GUI.Button(new Rect(rect.width - 29, 22 * i, 22, 22), "+"))
                {
                    list.Add(availalbe[i]);
                    availalbe.RemoveAt(i);
                    break;
                }
            }
                GUI.EndScrollView();
            EditorGUILayout.Space();
            GUILayout.Box(title, GUILayout.Width(Screen.width - 15 - Screen.width / 3f), GUILayout.Height(100));

            rect = GUILayoutUtility.GetLastRect();
            rect.y += 15;
            rect.height -= 15;
            m_scroll2 = GUI.BeginScrollView(rect, m_scroll2, new Rect(0, 0, rect.width, 22 * list.Count));
            for (int i = list.Count-1; i >= 0; i--)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), list[i].name);
                if (GUI.Button(new Rect(rect.width - 29, 22 * i, 22, 22), "x"))
                {
                    list.RemoveAt(i);
                }
            }
            GUI.EndScrollView();
        }
    }
}

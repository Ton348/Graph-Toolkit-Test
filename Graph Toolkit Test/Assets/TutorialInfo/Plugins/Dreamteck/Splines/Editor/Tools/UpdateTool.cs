using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace Dreamteck.Splines
{
    public class UpdateTool : SplineTool
    {
        protected GameObject m_obj;
        protected ObjectController m_spawner;
        private string m_updated = "";

        public override string GetName()
        {
            return "Update Components";
        }

        protected override string GetPrefix()
        {
            return "UpdateTool";
        }

        public override void Draw(Rect windowRect)
        {
            if (GUILayout.Button("Update All Spline Components"))
            {
                m_updated = "";
                UpdateComputers();
                UpdateNodes();
                UpdateUsers();
            }
            if (GUILayout.Button("Update SplineUsers"))
            {
                m_updated = "";
                UpdateUsers();
            }
            if (GUILayout.Button("Update MeshGenerators"))
            {
                m_updated = "";
                UpdateMeshGenerators();
            }
            if (GUILayout.Button("Update SplineComputers"))
            {
                m_updated = "";
                UpdateComputers();
            }
            if (GUILayout.Button("Update Nodes In Scene"))
            {
                m_updated = "";
                UpdateNodes();
            }

            EditorGUILayout.Space();
            GUILayout.Label(m_updated);
        }

        private void UpdateNodes()
        {
            Node[] nodes = GameObject.FindObjectsOfType<Node>();
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < nodes.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating nodes", "Updating node " + nodes[i].name, (float)i / (nodes.Length - 1));
                nodes[i].UpdateConnectedComputers();
                EditorUtility.SetDirty(nodes[i]);
                m_updated += i + " - " + nodes[i].name + System.Environment.NewLine;
            }
            EditorUtility.ClearProgressBar();
            if (nodes.Length == 0) m_updated += System.Environment.NewLine+"No active Nodes found in the scene.";
        }

        private void UpdateUsers()
        {
            SplineUser[] users = GameObject.FindObjectsOfType<SplineUser>();
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < users.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating users", "Updating user " + users[i].name, (float)i/(users.Length-1));
                users[i].Rebuild();
                EditorUtility.SetDirty(users[i]);
                m_updated += i + " - " + users[i].name + System.Environment.NewLine;
            }
            EditorUtility.ClearProgressBar();
            if (users.Length == 0) m_updated += System.Environment.NewLine+"No active SplineUsers found in the scene.";
        }

        private void UpdateMeshGenerators()
        {
            MeshGenerator[] users = GameObject.FindObjectsOfType<MeshGenerator>();
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < users.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating mesh generators", "Updating generator " + users[i].name, (float)i / (users.Length - 1));
                users[i].Rebuild();
                EditorUtility.SetDirty(users[i]);
                m_updated += i + " - " + users[i].name + System.Environment.NewLine;
            }
            EditorUtility.ClearProgressBar();
            if (users.Length == 0) m_updated += System.Environment.NewLine + "No active MeshGenerators found in the scene.";
        }

        private void UpdateComputers()
        {
            SplineComputer[] computers = GameObject.FindObjectsOfType<SplineComputer>();
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < computers.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Updating spline computers", "Updating computer " + computers[i].name, (float)i / (computers.Length - 1));
                computers[i].RebuildImmediate();
                EditorUtility.SetDirty(computers[i]);
                m_updated += i + " - " + computers[i].name + System.Environment.NewLine;
            }
            EditorUtility.ClearProgressBar();
            if (computers.Length == 0) m_updated += System.Environment.NewLine+"No active SplineComputers found in the scene.";
        }
    }
}

namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;

    [CustomEditor(typeof(Node), true)]
    [CanEditMultipleObjects]
    public class NodeEditor : Editor {
        private SplineComputer m_addComp = null;
        private int m_addPoint = 0;
        private Node m_lastnode = null;
        private int[] m_availablePoints;
        bool m_connectionsOpen = false, m_settingsOpen = false;

        private SerializedProperty m_transformNormals, m_transformSize, m_transformTangents, m_type;
        private Node[] m_nodes = new Node[0];
        private SerializedObject m_serializedNodes;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Node node = (Node)target;
            if (m_nodes.Length == 1)
            {
                if (m_addComp != null)
                {
                    string[] pointNames = new string[m_availablePoints.Length];
                    for (int i = 0; i < pointNames.Length; i++)
                    {
                        pointNames[i] = "Point " + (m_availablePoints[i] + 1);
                    }
                    if (m_availablePoints.Length > 0) m_addPoint = EditorGUILayout.Popup("Link point", m_addPoint, pointNames);
                    else EditorGUILayout.LabelField("No Points Available");

                    if (GUILayout.Button("Cancel"))
                    {
                        m_addComp = null;
                        m_addPoint = 0;
                    }
                    if (m_addPoint >= 0 && m_availablePoints.Length > m_addPoint)
                    {
                        if (node.HasConnection(m_addComp, m_availablePoints[m_addPoint])) EditorGUILayout.HelpBox("Connection already exists (" + m_addComp.name + "," + m_availablePoints[m_addPoint], MessageType.Error);
                        else if (GUILayout.Button("Link"))
                        {
                            AddConnection(m_addComp, m_availablePoints[m_addPoint]);
                        }
                    }
                }
                else
                {
                    SplineEditorGui.BeginContainerBox(ref m_connectionsOpen, "Connections");
                    if (m_connectionsOpen)
                    {
                        ConnectionsGui();
                    }
                    SplineEditorGui.EndContainerBox();

                    Rect rect = GUILayoutUtility.GetLastRect();
                    SplineComputer[] addComps;
                    SplineComputer lastComp = m_addComp;
                    bool dragged = DreamteckEditorGui.DropArea<SplineComputer>(rect, out addComps);
                    if (dragged && addComps.Length > 0)
                    {
                        SelectComputer(addComps[0]);
                    }

                    if (lastComp != m_addComp)
                    {
                        SceneView.RepaintAll();
                    }
                }
            } else
            {
                EditorGUILayout.HelpBox("Connection UI not available when multiple Nodes are selected.", MessageType.Info);
            }

            SplineEditorGui.BeginContainerBox(ref m_settingsOpen, "Settings");
            if (m_settingsOpen)
            {
                SettingsGui();
            }
            SplineEditorGui.EndContainerBox();
        }

        private void SettingsGui()
        {
            Node node = (Node)target;
            m_serializedNodes = new SerializedObject(m_nodes);
            m_transformNormals = m_serializedNodes.FindProperty("_transformNormals");
            m_transformSize = m_serializedNodes.FindProperty("_transformSize");
            m_transformTangents = m_serializedNodes.FindProperty("_transformTangents");
            m_type = m_serializedNodes.FindProperty("type");


            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_transformNormals, new GUIContent("Transform Normals"));
            EditorGUILayout.PropertyField(m_transformSize, new GUIContent("Transform Size"));
            EditorGUILayout.PropertyField(m_transformTangents, new GUIContent("Transform Tangents"));
            EditorGUILayout.PropertyField(m_type, new GUIContent("Node Type"));

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
                m_serializedNodes.ApplyModifiedProperties();
                node.UpdatePoints();
                node.UpdateConnectedComputers();
                SetDirty(node);
            }

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Align Tangents X"))
            {
                AlignTangents(node, 0);
            }

            if (GUILayout.Button("Align Tangents Y"))
            {
                AlignTangents(node, 1);
            }

            if (GUILayout.Button("Align Tangents Z"))
            {
                AlignTangents(node, 2);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AlignTangents(Node node, int axis)
        {
            Vector3 axisVector = Vector3.forward;
            switch (axis)
            {
                case 0: axisVector = node.transform.right; break;
                case 1: axisVector = node.transform.up; break;
                case 2: axisVector = node.transform.forward; break;
            }

            Undo.RecordObject(node, "Align Tangents");
            SplinePoint point = node.GetPoint(0, false);
            Vector3 tan1 = point.tangent - point.position;
            Vector3 tan2 = point.tangent2 - point.position;
            float tan1Dir = Mathf.Sign(Vector3.Dot(tan1, axisVector));
            float tan2Dir = Mathf.Sign(Vector3.Dot(tan2, axisVector));
            point.tangent = point.position + axisVector * tan1Dir * tan1.magnitude;
            point.tangent2 = point.position + axisVector * tan2Dir * tan2.magnitude;
            node.SetPoint(0, point, false);
            node.UpdateConnectedComputers();
            SetDirty(node);
            SceneView.RepaintAll();
        }

        void ConnectionsGui()
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
            EditorGUILayout.Space();

            if (connections.Length > 0)
            {
                for (int i = 0; i < connections.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(connections[i].spline.name + " at point " + (connections[i].pointIndex+1));
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                    {
                        Selection.activeGameObject = connections[i].spline.gameObject;
                    }
                    SplineEditorGui.SetHighlightColors(SplinePrefs.highlightColor, SplinePrefs.highlightContentColor);
                    if (SplineEditorGui.EditorLayoutSelectableButton(new GUIContent("Swap Tangents"), connections[i].spline.type == Spline.Type.Bezier, connections[i].invertTangents))
                    {
                        connections[i].invertTangents = !connections[i].invertTangents;
                        node.UpdateConnectedComputers();
                        SetDirty(node);
                        SceneView.RepaintAll();
                    }
                   
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        Undo.RecordObject(node, "Remove connection");
                        Undo.RecordObject(connections[i].spline, "Remove node");
                        connections[i].spline.DisconnectNode(connections[i].pointIndex);
                        node.RemoveConnection(connections[i].spline, connections[i].pointIndex);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else EditorGUILayout.HelpBox("Drag & Drop SplineComputers here to link their points.", MessageType.Info);
        }

        void OnEnable()
        {
            m_lastnode = (Node)target;
            m_lastnode.EditorMaintainConnections();
            m_connectionsOpen = EditorPrefs.GetBool("Dreamteck.Splines.Editor.NodeEditor.connectionsOpen");
            m_settingsOpen = EditorPrefs.GetBool("Dreamteck.Splines.Editor.NodeEditor.settingsOpen");
            m_nodes = new Node[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                m_nodes[i] = (Node)targets[i];
            }
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += DuringSceneGui;
#else
            SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool("Dreamteck.Splines.Editor.NodeEditor.connectionsOpen", m_connectionsOpen);
            EditorPrefs.SetBool("Dreamteck.Splines.Editor.NodeEditor.settingsOpen", m_settingsOpen);
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= DuringSceneGui;
#else
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
        }

        void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                if (((Node)target) == null)
                {
                    Node.Connection[] connections = m_lastnode.GetConnections();
                    for(int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].spline == null) continue;
                        Undo.RecordObject(connections[i].spline, "Delete node connections");
                    }
                    m_lastnode.ClearConnections();
                }
            }
        }

        void SelectComputer(SplineComputer comp)
        {
            m_addComp = comp;
            if (m_addComp != null) m_availablePoints = GetAvailablePoints(m_addComp);
            SceneView.RepaintAll();
            Repaint();
        }

        void AddConnection(SplineComputer computer, int pointIndex)
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
            if (EditorUtility.DisplayDialog("Link point?", "Add point " + (pointIndex+1) + " to connections?", "Yes", "No"))
            {
                Undo.RecordObject(m_addComp, "Add connection");
                Undo.RecordObject(node, "Add Connection");
                if (connections.Length == 0)
                {
                    switch (EditorUtility.DisplayDialogComplex("Align node to point?", "This is the first connection for the node, would you like to snap or align the node's Transform the spline point.", "No", "Snap", "Snap and Align"))
                    {
                        case 1: SplinePoint point = m_addComp.GetPoint(pointIndex);
                            node.transform.position = point.position;
                            break;
                        case 2:
                            SplineSample result = m_addComp.Evaluate(pointIndex);
                            node.transform.position = result.position;
                            node.transform.rotation = result.rotation;
                            break;
                    }
                }
                computer.ConnectNode(node, pointIndex);
                m_addComp = null;
                m_addPoint = 0;
                SetDirty(node);
                SceneView.RepaintAll();
                Repaint();
            }
        }

        int[] GetAvailablePoints(SplineComputer computer)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < computer.pointCount; i++)
            {
                if (computer.GetNode(i) != null) continue;
                indices.Add(i);
            }
            return indices.ToArray();
        }

        protected virtual void DuringSceneGui(SceneView current)
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
            for (int i = 0; i < connections.Length; i++)
            {
                DssplineDrawer.DrawSplineComputer(connections[i].spline, 0.0, 1.0, 0.5f);
            }

            if (m_addComp == null)
            {
                if (connections.Length > 0)
                {
                    bool bezier = false;
                    for (int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].spline == null) continue;
                        if (connections[i].spline.type == Spline.Type.Bezier)
                        {
                            bezier = true;
                            continue;
                        }
                    }
                    if (bezier && node.type == Node.Type.Smooth)  
                    {
                        if (connections[0].spline != null)
                        {
                            SplinePoint point = node.GetPoint(0, true);
                            Handles.DrawDottedLine(node.transform.position, point.tangent, 6f);
                            Handles.DrawDottedLine(node.transform.position, point.tangent2, 6f);
                            Vector3 lastPos = point.tangent;
                            bool setPoint = false;
                            point.SetTangentPosition(Handles.PositionHandle(point.tangent, node.transform.rotation));
                            if (lastPos != point.tangent) setPoint = true;
                            lastPos = point.tangent2;
                            point.SetTangent2Position(Handles.PositionHandle(point.tangent2, node.transform.rotation));
                            if (lastPos != point.tangent2) setPoint = true;

                            if (setPoint)
                            {
                                node.SetPoint(0, point, true);
                                node.UpdateConnectedComputers();
                                SetDirty(node);
                            }
                        }
                    }
                }
                return;
            }
            SplinePoint[] points = m_addComp.GetPoints();
            Transform camTransform = SceneView.currentDrawingSceneView.camera.transform;
            DssplineDrawer.DrawSplineComputer(m_addComp, 0.0, 1.0, 0.5f);
            TextAnchor originalAlignment = GUI.skin.label.alignment;
            Color originalColor = GUI.skin.label.normal.textColor;

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = m_addComp.editorPathColor;
            for (int i = 0; i < m_availablePoints.Length; i++)
            {
                if (m_addComp.isClosed && i == points.Length - 1) break;

                Handles.Label(points[i].position + Camera.current.transform.up * HandleUtility.GetHandleSize(points[i].position) * 0.3f, (i + 1).ToString());
                if (SplineEditorHandles.CircleButton(points[m_availablePoints[i]].position, Quaternion.LookRotation(-camTransform.forward, camTransform.up), HandleUtility.GetHandleSize(points[m_availablePoints[i]].position) * 0.1f, 2f, m_addComp.editorPathColor))
                {
                    AddConnection(m_addComp, m_availablePoints[i]);
                    break;
                }
            }
            GUI.skin.label.alignment = originalAlignment;
            GUI.skin.label.normal.textColor = originalColor;

        }

        public static void SetDirty(Node node)
        {
            EditorUtility.SetDirty(node);
            Node.Connection[] connections = node.GetConnections();
            for (int i = 0; i < connections.Length; i++)
            {
                EditorUtility.SetDirty(connections[i].spline);
            }
        }
    }
}
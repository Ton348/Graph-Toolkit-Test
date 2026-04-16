using System.Collections.Generic;
using Dreamteck.Editor;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class DreamteckSplinesEditor : SplineEditor
	{
		private readonly Toolbar m_nodesToolbar;

		private readonly Transform m_transform;

		private readonly List<Vector3> m_triggerWorldPositions = new();
		private DscreatePointModule m_createPointModule;
		public SplineComputer spline;

		public DreamteckSplinesEditor(SplineComputer splineComputer, SerializedObject splineHolder) : base(
			splineComputer.transform.localToWorldMatrix, splineHolder, "_spline")
		{
			spline = splineComputer;
			m_transform = spline.transform;
			evaluate = spline.Evaluate;
			evaluateAtPoint = spline.Evaluate;
			evaluatePosition = spline.EvaluatePosition;
			calculateLength = spline.CalculateLength;
			travel = spline.Travel;
			undoHandler = HandleUndo;
			mainModule.onBeforeDeleteSelectedPoints += OnBeforeDeleteSelectedPoints;
			mainModule.onDuplicatePoint += OnDuplicatePoint;
			if (spline.isNewlyCreated)
			{
				if (SplinePrefs.startInCreationMode)
				{
					open = true;
					editMode = true;
					ToggleModule(0);
				}

				spline.isNewlyCreated = false;
			}

			var nodeToolbarContents = new GUIContent[3];
			nodeToolbarContents[0] = new GUIContent("Select");
			nodeToolbarContents[1] = new GUIContent("Delete");
			nodeToolbarContents[2] = new GUIContent("Disconnect");
			m_nodesToolbar = new Toolbar(nodeToolbarContents);
		}

		public bool splineChanged { get; private set; }


		protected override string editorName => "DreamteckSplines";

		protected override void Load()
		{
			m_pointOperations.Add(new PointOperation
				{ name = "Center To Transform", action = delegate { CenterSelection(); } });
			m_pointOperations.Add(new PointOperation
				{ name = "Move Transform To", action = delegate { MoveTransformToSelection(); } });
			base.Load();
		}

		private void OnDuplicatePoint(int[] points)
		{
			for (var i = 0; i < points.Length; i++)
			{
				spline.ShiftNodes(points[i], spline.pointCount - 1, 1);
			}
		}

		public override void DrawInspector()
		{
			drawColor = spline.editorPathColor;
			is2D = spline.is2D;
			base.DrawInspector();
		}

		public override void DrawScene(SceneView current)
		{
			if (spline == null)
			{
				return;
			}

			drawColor = spline.editorPathColor;
			is2D = spline.is2D;
			base.DrawScene(current);
		}

		public void CacheTriggerPositions()
		{
			m_triggerWorldPositions.Clear();
			LoopTriggerProperties(trigger =>
			{
				SerializedProperty positionProperty = trigger.FindPropertyRelative("position");
				m_triggerWorldPositions.Add(spline.EvaluatePosition(positionProperty.floatValue));
			});
		}

		public void WriteTriggerPositions()
		{
			var projectSample = new SplineSample();
			var index = 0;
			LoopTriggerProperties(trigger =>
			{
				spline.Project(m_triggerWorldPositions[index], ref projectSample);
				SerializedProperty positionProperty = trigger.FindPropertyRelative("position");
				positionProperty.floatValue = (float)projectSample.percent;
				index++;
			});
			m_serializedObject.ApplyModifiedProperties();
		}

		private void OnBeforeDeleteSelectedPoints()
		{
			CacheTriggerPositions();
			var nodeString = "";
			var deleteNodes = new List<Node>();
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				Node node = spline.GetNode(selectedPoints[i]);
				if (node)
				{
					spline.DisconnectNode(selectedPoints[i]);
					if (node.GetConnections().Length == 0)
					{
						deleteNodes.Add(node);
						if (nodeString != "")
						{
							nodeString += ", ";
						}

						string trimmed = node.name.Trim();
						if (nodeString.Length + trimmed.Length > 80)
						{
							nodeString += "...";
						}
						else
						{
							nodeString += node.name.Trim();
						}
					}
				}
			}

			if (deleteNodes.Count > 0)
			{
				string message = "The following nodes:\r\n" + nodeString +
				                 "\r\n were only connected to the currently selected points. Would you like to remove them from the scene?";
				if (EditorUtility.DisplayDialog("Remove nodes?", message, "Yes", "No"))
				{
					for (var i = 0; i < deleteNodes.Count; i++)
					{
						Undo.DestroyObjectImmediate(deleteNodes[i].gameObject);
					}
				}
			}

			int min = spline.pointCount - 1;
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				if (selectedPoints[i] < min)
				{
					min = selectedPoints[i];
				}
			}
		}

		protected override void PointMenu()
		{
			base.PointMenu();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Nodes", GUILayout.MaxWidth(200f));
			var nodesCount = 0;
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				if (spline.GetNode(selectedPoints[i]) != null)
				{
					nodesCount++;
				}
			}

			if (nodesCount > 0)
			{
				int option = -1;
				m_nodesToolbar.center = false;
				m_nodesToolbar.Draw(ref option);
				if (option == 0)
				{
					var nodeList = new List<Node>();
					for (var i = 0; i < selectedPoints.Count; i++)
					{
						Node node = spline.GetNode(selectedPoints[i]);
						if (node != null)
						{
							nodeList.Add(node);
						}
					}

					Selection.objects = nodeList.ToArray();
				}

				if (option == 1)
				{
					for (var i = 0; i < selectedPoints.Count; i++)
					{
						var delete = true;
						Node node = spline.GetNode(selectedPoints[i]);
						if (node.GetConnections().Length > 1)
						{
							if (!EditorUtility.DisplayDialog("Delete Node",
								    "Node " + node.name +
								    " has multiple connections. Are you sure you want to completely remove it?", "Yes",
								    "No"))
							{
								delete = false;
							}
						}

						if (delete)
						{
							Undo.RegisterCompleteObjectUndo(spline, "Delete Node");
							Undo.DestroyObjectImmediate(node.gameObject);
							spline.DisconnectNode(selectedPoints[i]);
							EditorUtility.SetDirty(spline);
						}
					}
				}

				if (option == 2)
				{
					for (var i = 0; i < selectedPoints.Count; i++)
					{
						Undo.RegisterCompleteObjectUndo(spline, "Disconnect Node");
						spline.DisconnectNode(selectedPoints[i]);
						EditorUtility.SetDirty(spline);
					}
				}
			}
			else
			{
				if (GUILayout.Button(selectedPoints.Count == 1 ? "Add Node to Point" : "Add Nodes to Points"))
				{
					for (var i = 0; i < selectedPoints.Count; i++)
					{
						SplineSample sample = spline.Evaluate(selectedPoints[i]);
						var go = new GameObject(spline.name + "_Node_" + (spline.GetNodes().Count + 1));
						go.transform.parent = spline.transform;
						go.transform.position = sample.position;
						if (spline.is2D)
						{
							go.transform.rotation = sample.rotation * Quaternion.Euler(90, -90, 0);
						}
						else
						{
							go.transform.rotation = sample.rotation;
						}

						var node = go.AddComponent<Node>();
						Undo.RegisterCreatedObjectUndo(go, "Create Node");
						Undo.RegisterCompleteObjectUndo(spline, "Create Node");
						spline.ConnectNode(node, selectedPoints[i]);
					}
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		protected override void OnModuleList(List<PointModule> list)
		{
			m_createPointModule = new DscreatePointModule(this);
			list.Add(m_createPointModule);
			list.Add(new DeletePointModule(this));
			list.Add(new PointMoveModule(this));
			list.Add(new PointRotateModule(this));
			list.Add(new PointScaleModule(this));
			list.Add(new PointNormalModule(this));
			list.Add(new PointMirrorModule(this));
			list.Add(new PrimitivesModule(this));
		}

		public override void Destroy()
		{
			base.Destroy();
			if (spline != null)
			{
				spline.RebuildImmediate();
			}
		}

		public override void BeforeSceneGui(SceneView current)
		{
			for (var i = 0; i < moduleCount; i++)
			{
				SetupModule(GetModule(i));
			}

			SetupModule(mainModule);
			m_createPointModule.createPointColor = SplinePrefs.createPointColor;
			m_createPointModule.createPointSize = SplinePrefs.createPointSize;
			base.BeforeSceneGui(current);
		}

		public override void DeletePoint(int index)
		{
			CacheTriggerPositions();
			var nodes = new Dictionary<int, Node>();
			foreach (KeyValuePair<int, Node> node in spline.GetNodes())
			{
				if (node.Key > index)
				{
					spline.DisconnectNode(node.Key);
					nodes.Add(node.Key - 1, node.Value);
				}
			}

			SerializedProperty nodesProperty = m_serializedObject.FindProperty("_nodes");
			for (var i = 0; i < nodesProperty.arraySize; i++)
			{
				SerializedProperty indexProperty =
					nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("pointIndex");
				if (indexProperty.intValue > index)
				{
					nodesProperty.DeleteArrayElementAtIndex(i);
					i--;
				}
			}

			InverseTransformPoints();
			pointsProperty.DeleteArrayElementAtIndex(index);

			foreach (KeyValuePair<int, Node> node in nodes)
			{
				spline.ConnectNode(node.Value, node.Key);
				nodesProperty.arraySize = nodesProperty.arraySize + 1;
				SerializedProperty lastProperty = nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1);
				SerializedProperty lastnodeProperty = lastProperty.FindPropertyRelative("node");
				SerializedProperty lastIndexProperty = lastProperty.FindPropertyRelative("pointIndex");
				lastnodeProperty.objectReferenceValue = node.Value;
				lastIndexProperty.intValue = node.Key;
			}

			m_serializedObject.ApplyModifiedProperties();
			GetPointsFromSpline();
			spline.Rebuild(true);
			WriteTriggerPositions();
		}

		public override void GetPointsFromSpline()
		{
			base.GetPointsFromSpline();

			if (m_serializedObject.FindProperty("_space").enumValueIndex == (int)SplineComputer.Space.Local)
			{
				TransformPoints();
			}
		}

		public override void ApplyModifiedProperties(bool forceAllUpdate = false)
		{
			if (m_serializedObject.FindProperty("_space").enumValueIndex == (int)SplineComputer.Space.Local)
			{
				InverseTransformPoints();
			}

			for (var i = 0; i < points.Length; i++)
			{
				if (points[i].changed || forceAllUpdate)
				{
					spline.EditorSetPointDirty(i);
				}
			}

			splineChanged = true;

			if (spline.isClosed && points.Length < 3)
			{
				SetSplineClosed(false);
			}

			m_serializedObject.FindProperty("_is2D").boolValue = is2D;

			base.ApplyModifiedProperties(forceAllUpdate);

			spline.EditorUpdateConnectedNodes();

			if (m_serializedObject.FindProperty("editorUpdateMode").enumValueIndex ==
			    (int)SplineComputer.EditorUpdateMode.Default)
			{
				spline.RebuildImmediate(true, forceAllUpdate);
			}

			GetPointsFromSpline();
		}

		public override void SetSplineClosed(bool closed)
		{
			base.SetSplineClosed(closed);
			if (closed)
			{
				spline.Close();
			}
			else
			{
				if (selectedPoints.Count > 0)
				{
					spline.Break(selectedPoints[selectedPoints.Count - 1]);
				}
				else
				{
					spline.Break();
				}
			}
		}

		public override void UndoRedoPerformed()
		{
			base.UndoRedoPerformed();
			spline.RebuildImmediate(true, true);
		}

		private void TransformPoints()
		{
			m_matrix = spline.transform.localToWorldMatrix;
			for (var i = 0; i < points.Length; i++)
			{
				bool changed = points[i].changed;
				points[i].position = m_matrix.MultiplyPoint3x4(points[i].position);
				points[i].tangent = m_matrix.MultiplyPoint3x4(points[i].tangent);
				points[i].tangent2 = m_matrix.MultiplyPoint3x4(points[i].tangent2);
				points[i].normal = m_matrix.MultiplyVector(points[i].normal);
				points[i].changed = changed;
			}
		}

		private void InverseTransformPoints()
		{
			m_matrix = spline.transform.localToWorldMatrix;
			Matrix4x4 invMatrix = m_matrix.inverse;
			for (var i = 0; i < points.Length; i++)
			{
				bool changed = points[i].changed;
				points[i].position = invMatrix.MultiplyPoint3x4(points[i].position);
				points[i].tangent = invMatrix.MultiplyPoint3x4(points[i].tangent);
				points[i].tangent2 = invMatrix.MultiplyPoint3x4(points[i].tangent2);
				points[i].normal = invMatrix.MultiplyVector(points[i].normal);
				points[i].changed = changed;
			}
		}

		public override void SetPreviewPoints(SplinePoint[] points)
		{
			spline.SetPoints(points);
		}

		private void HandleUndo(string title)
		{
			Undo.RecordObject(spline, title);
		}

		public void MoveTransformToSelection()
		{
			Undo.RecordObject(m_transform, "Move Transform To");
			Vector3 avg = Vector3.zero;
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				avg += points[selectedPoints[i]].position;
			}

			avg /= selectedPoints.Count;
			m_transform.position = avg;
			ApplyModifiedProperties(true);
			ResetCurrentModule();
		}

		public void CenterSelection()
		{
			RecordUndo("Center Selection");
			Vector3 avg = Vector3.zero;
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				avg += points[selectedPoints[i]].position;
			}

			avg /= selectedPoints.Count;
			Vector3 delta = m_transform.position - avg;
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				points[selectedPoints[i]].SetPosition(points[selectedPoints[i]].position + delta);
			}

			ApplyModifiedProperties(true);
			ResetCurrentModule();
		}

		private void SetupModule(PointModule module)
		{
			module.duplicationDirection = SplinePrefs.duplicationDirection;
			module.highlightColor = SplinePrefs.highlightColor;
			module.showPointNumbers = SplinePrefs.showPointNumbers;
		}
	}
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class DscreatePointModule : CreatePointModule
	{
		private readonly DreamteckSplinesEditor m_dsEditor;
		private bool m_createNode;

		public DscreatePointModule(SplineEditor editor) : base(editor)
		{
			m_dsEditor = (DreamteckSplinesEditor)editor;
		}

		public override void LoadState()
		{
			base.LoadState();
			m_createNode = LoadBool("createNode");
		}

		public override void SaveState()
		{
			base.SaveState();
			SaveBool("createNode", m_createNode);
		}

		protected override void OnDrawInspector()
		{
			base.OnDrawInspector();
			m_createNode = EditorGUILayout.Toggle("Create Node", m_createNode);
		}

		protected override void CreateSplinePoint(Vector3 position, Vector3 normal)
		{
			GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
			var indices = new List<int>();
			var nodes = new List<Node>();
			SplineComputer spline = m_dsEditor.spline;

			m_dsEditor.CacheTriggerPositions();

			if (!isClosed && points.Length >= 3)
			{
				Vector2 first = HandleUtility.WorldToGUIPoint(points[0].position);
				Vector2 last = HandleUtility.WorldToGUIPoint(position);
				if (Vector2.Distance(first, last) <= 20f)
				{
					if (EditorUtility.DisplayDialog("Close spline?", "Do you want to make the spline path closed ?",
						    "Yes", "No"))
					{
						m_editor.SetSplineClosed(true);
						spline.EditorSetAllPointsDirty();
						RegisterChange();
						SceneView.currentDrawingSceneView.Focus();
						SceneView.RepaintAll();
						return;
					}
				}
			}

			AddPoint();

			if (appendMode == AppendMode.End)
			{
				for (var i = 0; i < indices.Count; i++)
				{
					nodes[i].AddConnection(spline, indices[i] + 1);
				}
			}

			m_dsEditor.ApplyModifiedProperties(true);
			m_dsEditor.WriteTriggerPositions();
			RegisterChange();
			if (appendMode == AppendMode.Beginning)
			{
				spline.ShiftNodes(0, spline.pointCount - 1, 1);
			}

			if (m_createNode)
			{
				m_dsEditor.ApplyModifiedProperties();
				if (appendMode == 0)
				{
					CreateNodeForPoint(0);
				}
				else
				{
					CreateNodeForPoint(points.Length - 1);
				}
			}
		}

		protected override void InsertMode(Vector3 screenCoordinates)
		{
			base.InsertMode(screenCoordinates);
			double percent = ProjectScreenSpace(screenCoordinates);
			m_editor.evaluate(percent, ref m_evalResult);
			if (m_editor.eventModule.mouseRight)
			{
				SplineEditorHandles.DrawCircle(m_evalResult.position,
					Quaternion.LookRotation(m_editorCamera.transform.position - m_evalResult.position),
					HandleUtility.GetHandleSize(m_evalResult.position) * 0.2f);
				return;
			}

			if (SplineEditorHandles.CircleButton(m_evalResult.position,
				    Quaternion.LookRotation(m_editorCamera.transform.position - m_evalResult.position),
				    HandleUtility.GetHandleSize(m_evalResult.position) * 0.2f, 1.5f, color))
			{
				m_dsEditor.CacheTriggerPositions();
				var newPoint = new SplinePoint(m_evalResult.position, m_evalResult.position);
				newPoint.size = m_evalResult.size;
				newPoint.color = m_evalResult.color;
				newPoint.normal = m_evalResult.up;


				int pointIndex = m_dsEditor.spline.PercentToPointIndex(percent);
				m_editor.AddPointAt(pointIndex + 1);
				points[pointIndex + 1].SetPoint(newPoint);
				SplineComputer spline = m_dsEditor.spline;
				m_lastCreated = points.Length - 1;
				m_editor.ApplyModifiedProperties(true);
				spline.ShiftNodes(pointIndex + 1, spline.pointCount - 1, 1);
				if (m_createNode)
				{
					CreateNodeForPoint(pointIndex + 1);
				}

				RegisterChange();
				m_dsEditor.WriteTriggerPositions();
			}
		}

		private void CreateNodeForPoint(int index)
		{
			var obj = new GameObject("Node_" + (points.Length - 1));
			obj.transform.parent = m_dsEditor.spline.transform;
			var node = obj.AddComponent<Node>();
			node.transform.localRotation = Quaternion.identity;
			node.transform.position = points[index].position;
			Undo.SetCurrentGroupName("Create Node For Point " + index);
			Undo.RegisterCreatedObjectUndo(obj, "Create Node object");
			Undo.RegisterCompleteObjectUndo(m_dsEditor.spline, "Link Node");
			m_dsEditor.spline.ConnectNode(node, index);
		}
	}
}
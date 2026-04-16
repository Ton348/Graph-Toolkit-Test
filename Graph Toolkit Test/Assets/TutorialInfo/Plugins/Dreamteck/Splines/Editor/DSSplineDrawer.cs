using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamteck.Splines.Editor
{
	[InitializeOnLoad]
	public static class DssplineDrawer
	{
		private static bool s_refreshComputers;
		private static readonly List<SplineComputer> s_drawComputers = new();
		private static Vector3[] s_positions = new Vector3[0];
		private static Scene s_currentScene;

		static DssplineDrawer()
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += AutoDrawComputers;
#else
            SceneView.onSceneGUIDelegate += AutoDrawComputers;
#endif

			FindComputers();
			EditorApplication.hierarchyChanged += HerarchyWindowChanged;
			EditorApplication.playModeStateChanged += ModeChanged;
		}


		private static void ModeChanged(PlayModeStateChange stateChange)
		{
			s_refreshComputers = true;
		}

		private static void HerarchyWindowChanged()
		{
			if (s_currentScene != EditorSceneManager.GetActiveScene())
			{
				s_currentScene = EditorSceneManager.GetActiveScene();
				FindComputers();
			}
		}

		private static void FindComputers()
		{
			s_drawComputers.Clear();
			SplineComputer[] computers = GameObject.FindObjectsOfType<SplineComputer>();
			s_drawComputers.AddRange(computers);
		}

		private static void AutoDrawComputers(SceneView current)
		{
			if (s_refreshComputers)
			{
				s_refreshComputers = false;
				FindComputers();
			}

			for (var i = 0; i < s_drawComputers.Count; i++)
			{
				if (!s_drawComputers[i].editorAlwaysDraw)
				{
					s_drawComputers.RemoveAt(i);
					i--;
					continue;
				}

				DrawSplineComputer(s_drawComputers[i]);
			}
		}

		public static void RegisterComputer(SplineComputer comp)
		{
			if (s_drawComputers.Contains(comp))
			{
				return;
			}

			comp.editorAlwaysDraw = true;
			s_drawComputers.Add(comp);
		}

		public static void UnregisterComputer(SplineComputer comp)
		{
			for (var i = 0; i < s_drawComputers.Count; i++)
			{
				if (s_drawComputers[i] == comp)
				{
					s_drawComputers[i].editorAlwaysDraw = false;
					s_drawComputers.RemoveAt(i);
					return;
				}
			}
		}

		public static void DrawSplineComputer(
			SplineComputer comp,
			double fromPercent = 0.0,
			double toPercent = 1.0,
			float alpha = 1f)
		{
			if (comp == null)
			{
				return;
			}

			if (comp.pointCount < 2)
			{
				return;
			}

			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			Color prevColor = Handles.color;
			Color handleColor = comp.editorPathColor;
			handleColor.a = alpha;
			Handles.color = handleColor;

			if (comp.type == Spline.Type.BSpline && comp.pointCount > 1)
			{
				SplinePoint[] compPoints = comp.GetPoints();
				Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.5f * alpha);
				for (var i = 0; i < compPoints.Length - 1; i++)
				{
					Handles.DrawLine(compPoints[i].position, compPoints[i + 1].position);
				}

				Handles.color = handleColor;
			}

			if (!comp.editorDrawThickness)
			{
				if (s_positions.Length != comp.sampleCount * 2)
				{
					s_positions = new Vector3[comp.sampleCount * 2];
				}

				Vector3 prevPoint = comp.EvaluatePosition(fromPercent);
				var pointIndex = 0;
				for (var i = 1; i < comp.sampleCount; i++)
				{
					s_positions[pointIndex] = prevPoint;
					pointIndex++;
					s_positions[pointIndex] = comp[i].position;
					pointIndex++;
					prevPoint = s_positions[pointIndex - 1];
				}

				Handles.DrawLines(s_positions);
			}
			else
			{
				Transform editorCamera = SceneView.currentDrawingSceneView.camera.transform;
				if (s_positions.Length != comp.sampleCount * 6)
				{
					s_positions = new Vector3[comp.sampleCount * 6];
				}

				SplineSample prevResult = comp.Evaluate(fromPercent);
				Vector3 prevNormal = prevResult.up;
				if (comp.editorBillboardThickness)
				{
					prevNormal = (editorCamera.position - prevResult.position).normalized;
				}

				Vector3 prevRight = Vector3.Cross(prevResult.forward, prevNormal).normalized * prevResult.size * 0.5f;
				var pointIndex = 0;
				for (var i = 1; i < comp.sampleCount; i++)
				{
					Vector3 newNormal = comp[i].up;
					if (comp.editorBillboardThickness)
					{
						newNormal = (editorCamera.position - comp[i].position).normalized;
					}

					Vector3 newRight = Vector3.Cross(comp[i].forward, newNormal).normalized * comp[i].size * 0.5f;

					s_positions[pointIndex] = prevResult.position + prevRight;
					s_positions[pointIndex + comp.sampleCount * 2] = prevResult.position - prevRight;
					s_positions[pointIndex + comp.sampleCount * 4] = comp[i].position - newRight;
					pointIndex++;
					s_positions[pointIndex] = comp[i].position + newRight;
					s_positions[pointIndex + comp.sampleCount * 2] = comp[i].position - newRight;
					s_positions[pointIndex + comp.sampleCount * 4] = comp[i].position + newRight;
					pointIndex++;
					prevResult = comp[i];
					prevRight = newRight;
					prevNormal = newNormal;
				}

				Handles.DrawLines(s_positions);
			}

			Handles.color = prevColor;
		}
	}
}
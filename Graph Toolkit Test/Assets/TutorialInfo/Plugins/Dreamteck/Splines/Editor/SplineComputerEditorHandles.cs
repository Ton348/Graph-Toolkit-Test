using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public static class SplineComputerEditorHandles
	{
		public enum SplineSliderGizmo
		{
			ForwardTriangle,
			BackwardTriangle,
			DualArrow,
			Rectangle,
			Circle
		}

		private static SplineSample s_evalResult;

		public static bool Slider(
			SplineComputer spline,
			ref double percent,
			Color color,
			string text = "",
			SplineSliderGizmo gizmo = SplineSliderGizmo.Rectangle,
			float buttonSize = 1f)
		{
			Camera cam = SceneView.currentDrawingSceneView.camera;
			spline.Evaluate(percent, ref s_evalResult);
			float size = HandleUtility.GetHandleSize(s_evalResult.position);

			Handles.color = new Color(color.r, color.g, color.b, 0.4f);
			Handles.DrawSolidDisc(s_evalResult.position, cam.transform.position - s_evalResult.position,
				size * 0.2f * buttonSize);
			Handles.color = Color.white;
			if ((color.r + color.g + color.b + color.a) / 4f >= 0.9f)
			{
				Handles.color = Color.black;
			}

			Vector3 center = s_evalResult.position;
			Vector2 screenPosition = HandleUtility.WorldToGUIPoint(center);
			screenPosition.y += 20f;
			Vector3 localPos = cam.transform.InverseTransformPoint(center);
			if (text != "" && localPos.z > 0f)
			{
				Handles.BeginGUI();
				DreamteckEditorGui.Label(new Rect(screenPosition.x - 120 + text.Length * 4, screenPosition.y, 120, 25),
					text);
				Handles.EndGUI();
			}

			bool buttonClick = SplineEditorHandles.SliderButton(center, false, Color.white, 0.3f);
			Vector3 lookAtCamera = (cam.transform.position - s_evalResult.position).normalized;
			Vector3 right = Vector3.Cross(lookAtCamera, s_evalResult.forward).normalized * size * 0.1f * buttonSize;
			Vector3 front = Vector3.forward;
			switch (gizmo)
			{
				case SplineSliderGizmo.BackwardTriangle:
					center += s_evalResult.forward * size * 0.06f * buttonSize;
					front = center - s_evalResult.forward * size * 0.2f * buttonSize;
					Handles.DrawLine(center + right, front);
					Handles.DrawLine(front, center - right);
					Handles.DrawLine(center - right, center + right);
					break;

				case SplineSliderGizmo.ForwardTriangle:
					center -= s_evalResult.forward * size * 0.06f * buttonSize;
					front = center + s_evalResult.forward * size * 0.2f * buttonSize;
					Handles.DrawLine(center + right, front);
					Handles.DrawLine(front, center - right);
					Handles.DrawLine(center - right, center + right);
					break;

				case SplineSliderGizmo.DualArrow:
					center += s_evalResult.forward * size * 0.025f * buttonSize;
					front = center + s_evalResult.forward * size * 0.17f * buttonSize;
					Handles.DrawLine(center + right, front);
					Handles.DrawLine(front, center - right);
					Handles.DrawLine(center - right, center + right);
					center -= s_evalResult.forward * size * 0.05f * buttonSize;
					front = center - s_evalResult.forward * size * 0.17f * buttonSize;
					Handles.DrawLine(center + right, front);
					Handles.DrawLine(front, center - right);
					Handles.DrawLine(center - right, center + right);
					break;
				case SplineSliderGizmo.Rectangle:

					break;

				case SplineSliderGizmo.Circle:
					Handles.DrawWireDisc(center, lookAtCamera, 0.13f * size * buttonSize);
					break;
			}

			Vector3 lastPos = s_evalResult.position;
			Handles.color = Color.clear;
			Quaternion lookRotation = Quaternion.LookRotation(cam.transform.position - s_evalResult.position);
			s_evalResult.position = SplineEditorHandles.FreeMoveHandle(s_evalResult.position, size * 0.2f * buttonSize,
				Vector3.zero, Handles.CircleHandleCap);
			if (s_evalResult.position != lastPos)
			{
				percent = spline.Project(s_evalResult.position).percent;
			}

			Handles.color = Color.white;
			return buttonClick;
		}
	}
}
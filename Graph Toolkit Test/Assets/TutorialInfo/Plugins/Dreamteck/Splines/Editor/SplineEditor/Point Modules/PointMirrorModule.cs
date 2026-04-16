using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class PointMirrorModule : PointTransformModule
	{
		public enum Axis
		{
			X,
			Y,
			Z
		}

		public Axis axis = Axis.X;
		public bool flip;
		private Vector3 m_mirrorCenter = Vector3.zero;


		private SplinePoint[] m_mirrored = new SplinePoint[0];
		public float weldDistance;


		public PointMirrorModule(SplineEditor editor) : base(editor)
		{
			LoadState();
		}

		public override GUIContent GetIconOff()
		{
			return IconContent("||", "mirror", "Mirror Path");
		}

		public override GUIContent GetIconOn()
		{
			return IconContent("||", "mirror_on", "Mirror Path");
		}

		public override void LoadState()
		{
			axis = (Axis)LoadInt("axis");
			flip = LoadBool("flip");
			weldDistance = LoadFloat("weldDistance");
		}

		public override void SaveState()
		{
			base.SaveState();
			SaveInt("axis", (int)axis);
			SaveBool("flip", flip);
			SaveFloat("weldDistance", weldDistance);
		}

		public override void Select()
		{
			base.Select();
			ClearSelection();
			DoMirror();
			SetDirty();
		}

		public override void Deselect()
		{
			if (IsDirty())
			{
				if (EditorUtility.DisplayDialog("Unapplied Mirror Operation",
					    "There is an unapplied mirror operation. Do you want to apply the changes?", "Apply", "Revert"))
				{
					Apply();
				}
				else
				{
					Revert();
				}
			}

			base.Deselect();
		}

		protected override void OnDrawInspector()
		{
			if (selectedPoints.Count > 0)
			{
				ClearSelection();
			}

			EditorGUI.BeginChangeCheck();
			axis = (Axis)EditorGUILayout.EnumPopup("Axis", axis);
			flip = EditorGUILayout.Toggle("Flip", flip);
			weldDistance = EditorGUILayout.FloatField("Weld Distance", weldDistance);
			m_mirrorCenter = EditorGUILayout.Vector3Field("Center", m_mirrorCenter);
			if (EditorGUI.EndChangeCheck())
			{
				DoMirror();
			}

			if (IsDirty())
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Apply"))
				{
					Apply();
				}

				if (GUILayout.Button("Revert"))
				{
					Revert();
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		protected override void OnDrawScene()
		{
			if (selectedPoints.Count > 0)
			{
				ClearSelection();
			}

			Vector3 worldCenter = TransformPosition(m_mirrorCenter);
			Vector3 lastCenter = worldCenter;
			worldCenter = Handles.PositionHandle(worldCenter, m_rotation);
			m_mirrorCenter = InverseTransformPosition(worldCenter);
			DrawMirror();
			if (lastCenter != worldCenter)
			{
				DoMirror();
			}

			selectedPoints.Clear();
		}

		public void DoMirror()
		{
			List<int> half = GetHalf(ref m_originalPoints);
			int welded = -1;
			if (half.Count > 0)
			{
				if (flip)
				{
					if (IsWeldable(m_originalPoints[half[0]]))
					{
						welded = half[0];
						half.RemoveAt(0);
					}
				}
				else
				{
					if (IsWeldable(m_originalPoints[half[half.Count - 1]]))
					{
						welded = half[half.Count - 1];
						half.RemoveAt(half.Count - 1);
					}
				}

				int offset = welded >= 0 ? 1 : 0;
				int mirroredLength = half.Count * 2 + offset;
				if (m_mirrored.Length != mirroredLength)
				{
					m_mirrored = new SplinePoint[mirroredLength];
				}

				for (var i = 0; i < half.Count; i++)
				{
					if (flip)
					{
						m_mirrored[i] = new SplinePoint(m_originalPoints[half[half.Count - 1 - i]]);
						m_mirrored[i + half.Count + offset] = GetMirrored(m_originalPoints[half[i]]);
						SwapTangents(ref m_mirrored[i]);
						SwapTangents(ref m_mirrored[i + half.Count + offset]);
					}
					else
					{
						m_mirrored[i] = new SplinePoint(m_originalPoints[half[i]]);
						m_mirrored[i + half.Count + offset] = GetMirrored(m_originalPoints[half[half.Count - 1 - i]]);
					}
				}

				if (welded >= 0)
				{
					m_mirrored[half.Count] = new SplinePoint(m_originalPoints[welded]);
					if (flip)
					{
						SwapTangents(ref m_mirrored[half.Count]);
					}

					MakeMiddlePoint(ref m_mirrored[half.Count]);
				}

				if (isClosed && m_mirrored.Length > 0)
				{
					MakeMiddlePoint(ref m_mirrored[0]);
					m_mirrored[m_mirrored.Length - 1] = new SplinePoint(m_mirrored[0]);
				}
			}
			else
			{
				m_mirrored = new SplinePoint[0];
			}

			m_editor.SetPointsArray(m_mirrored);
			RegisterChange();
			SetDirty();
		}

		private void SwapTangents(ref SplinePoint point)
		{
			Vector3 temp = point.tangent;
			point.tangent = point.tangent2;
			point.tangent2 = temp;
		}

		private void MakeMiddlePoint(ref SplinePoint point)
		{
			point.type = SplinePoint.Type.Broken;
			InverseTransformPoint(ref point);
			Vector3 newPos = point.position;
			switch (axis)
			{
				case Axis.X:

					newPos.x = m_mirrorCenter.x;
					point.SetPosition(newPos);
					if ((point.tangent.x >= m_mirrorCenter.x && flip) || (point.tangent.x <= m_mirrorCenter.x && !flip))
					{
						point.tangent2 = point.tangent;
						point.SetTangent2X(point.position.x + (point.position.x - point.tangent.x));
					}
					else
					{
						point.tangent = point.tangent2;
						point.SetTangentX(point.position.x + (point.position.x - point.tangent2.x));
					}

					break;
				case Axis.Y:
					newPos.y = m_mirrorCenter.y;
					point.SetPosition(newPos);
					if ((point.tangent.y >= m_mirrorCenter.y && flip) || (point.tangent.y <= m_mirrorCenter.y && !flip))
					{
						point.tangent2 = point.tangent;
						point.SetTangent2Y(point.position.y + (point.position.y - point.tangent.y));
					}
					else
					{
						point.tangent = point.tangent2;
						point.SetTangentY(point.position.y + (point.position.y - point.tangent2.y));
					}

					break;
				case Axis.Z:
					newPos.z = m_mirrorCenter.z;
					point.SetPosition(newPos);
					if ((point.tangent.z >= m_mirrorCenter.z && flip) || (point.tangent.z <= m_mirrorCenter.z && !flip))
					{
						point.tangent2 = point.tangent;
						point.SetTangent2Z(point.position.z + (point.position.z - point.tangent.z));
					}
					else
					{
						point.tangent = point.tangent2;
						point.SetTangentZ(point.position.z + (point.position.z - point.tangent2.z));
					}

					break;
			}

			TransformPoint(ref point);
		}

		private bool IsWeldable(SplinePoint point)
		{
			switch (axis)
			{
				case Axis.X:
					if (Mathf.Abs(point.position.x - m_mirrorCenter.x) <= weldDistance)
					{
						return true;
					}

					break;
				case Axis.Y:
					if (Mathf.Abs(point.position.y - m_mirrorCenter.y) <= weldDistance)
					{
						return true;
					}

					break;
				case Axis.Z:
					if (Mathf.Abs(point.position.z - m_mirrorCenter.z) <= weldDistance)
					{
						return true;
					}

					break;
			}

			return false;
		}

		private void DrawMirror()
		{
			var points = new Vector3[4];
			Color color = Color.white;
			Vector3 worldCenter = TransformPosition(m_mirrorCenter);
			float size = HandleUtility.GetHandleSize(worldCenter);
			Vector3 forward = m_rotation * Vector3.forward * size;
			Vector3 back = -forward;
			Vector3 right = m_rotation * Vector3.right * size;
			Vector3 left = -right;
			Vector3 up = m_rotation * Vector3.up * size;
			Vector3 down = -up;
			switch (axis)
			{
				case Axis.X:
					points[0] = back + up;
					points[1] = forward + up;
					points[2] = forward + down;
					points[3] = back + down;
					color = Color.red;
					break;
				case Axis.Y:
					points[0] = back + left;
					points[1] = forward + left;
					points[2] = forward + right;
					points[3] = back + right;
					color = Color.green;
					break;
				case Axis.Z:
					points[0] = left + up;
					points[1] = right + up;
					points[2] = right + down;
					points[3] = left + down;
					color = Color.blue;
					break;
			}

			Handles.color = color;
			Handles.DrawLine(worldCenter + points[0], worldCenter + points[1]);
			Handles.DrawLine(worldCenter + points[1], worldCenter + points[2]);
			Handles.DrawLine(worldCenter + points[2], worldCenter + points[3]);
			Handles.DrawLine(worldCenter + points[3], worldCenter + points[0]);
			Handles.color = Color.white;
		}

		private SplinePoint GetMirrored(SplinePoint source)
		{
			var newPoint = new SplinePoint(source);
			InverseTransformPoint(ref newPoint);
			switch (axis)
			{
				case Axis.X:
					newPoint.SetPositionX(m_mirrorCenter.x - (newPoint.position.x - m_mirrorCenter.x));
					newPoint.SetNormalX(-newPoint.normal.x);
					newPoint.SetTangentX(m_mirrorCenter.x - (newPoint.tangent.x - m_mirrorCenter.x));
					newPoint.SetTangent2X(m_mirrorCenter.x - (newPoint.tangent2.x - m_mirrorCenter.x));
					break;
				case Axis.Y:
					newPoint.SetPositionY(m_mirrorCenter.y - (newPoint.position.y - m_mirrorCenter.y));
					newPoint.SetNormalY(-newPoint.normal.y);
					newPoint.SetTangentY(m_mirrorCenter.y - (newPoint.tangent.y - m_mirrorCenter.y));
					newPoint.SetTangent2Y(m_mirrorCenter.y - (newPoint.tangent2.y - m_mirrorCenter.y));
					break;
				case Axis.Z:
					newPoint.SetPositionZ(m_mirrorCenter.z - (newPoint.position.z - m_mirrorCenter.z));
					newPoint.SetNormalZ(-newPoint.normal.z);
					newPoint.SetTangentZ(m_mirrorCenter.z - (newPoint.tangent.z - m_mirrorCenter.z));
					newPoint.SetTangent2Z(m_mirrorCenter.z - (newPoint.tangent2.z - m_mirrorCenter.z));
					break;
			}

			SwapTangents(ref newPoint);
			TransformPoint(ref newPoint);
			return newPoint;
		}


		private List<int> GetHalf(ref SplinePoint[] points)
		{
			var found = new List<int>();
			switch (axis)
			{
				case Axis.X:

					for (var i = 0; i < points.Length; i++)
					{
						if (flip)
						{
							if (InverseTransformPosition(points[i].position).x >= m_mirrorCenter.x)
							{
								found.Add(i);
							}
						}
						else
						{
							if (InverseTransformPosition(points[i].position).x <= m_mirrorCenter.x)
							{
								found.Add(i);
							}
						}
					}

					break;

				case Axis.Y:
					for (var i = 0; i < points.Length; i++)
					{
						if (flip)
						{
							if (InverseTransformPosition(points[i].position).y >= m_mirrorCenter.y)
							{
								found.Add(i);
							}
							else
							{
								if (InverseTransformPosition(points[i].position).y <= m_mirrorCenter.y)
								{
									found.Add(i);
								}
							}
						}
					}

					break;
				case Axis.Z:
					for (var i = 0; i < points.Length; i++)
					{
						if (flip)
						{
							if (InverseTransformPosition(points[i].position).z >= m_mirrorCenter.z)
							{
								found.Add(i);
							}
						}
						else
						{
							if (InverseTransformPosition(points[i].position).z <= m_mirrorCenter.z)
							{
								found.Add(i);
							}
						}
					}

					break;
			}

			return found;
		}
	}
}
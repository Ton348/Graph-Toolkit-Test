using System;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines
{
	[ExecuteInEditMode]
	[AddComponentMenu("Dreamteck/Splines/Node Connector")]
	public class Node : MonoBehaviour
	{
		public enum Type
		{
			Smooth,
			Free
		}

		[HideInInspector]
		public Type type = Type.Smooth;

		[SerializeField]
		[HideInInspector]
		protected Connection[] m_connections = new Connection[0];

		[SerializeField]
		[HideInInspector]
		private bool m_transformSize = true;

		[SerializeField]
		[HideInInspector]
		private bool m_transformNormals = true;

		[SerializeField]
		[HideInInspector]
		private bool m_transformTangents = true;

		private Vector3 m_lastPosition, m_lastScale;
		private Quaternion m_lastRotation;
		private Transform m_trs;

		public bool transformNormals
		{
			get => m_transformNormals;
			set
			{
				if (value != m_transformNormals)
				{
					m_transformNormals = value;
					UpdatePoints();
				}
			}
		}

		public bool transformSize
		{
			get => m_transformSize;
			set
			{
				if (value != m_transformSize)
				{
					m_transformSize = value;
					UpdatePoints();
				}
			}
		}

		public bool transformTangents
		{
			get => m_transformTangents;
			set
			{
				if (value != m_transformTangents)
				{
					m_transformTangents = value;
					UpdatePoints();
				}
			}
		}

		private void Awake()
		{
			m_trs = transform;
			SampleTransform();
		}

		private void Update()
		{
			Run();
		}


		private void LateUpdate()
		{
			Run();
		}

		private void OnDestroy()
		{
			ClearConnections();
		}

		private bool TransformChanged()
		{
#if UNITY_EDITOR
			if (m_trs == null)
			{
				return m_lastPosition != transform.position || m_lastRotation != transform.rotation ||
				       m_lastScale != transform.lossyScale;
			}
#endif
			return m_lastPosition != m_trs.position || m_lastRotation != m_trs.rotation ||
			       m_lastScale != m_trs.lossyScale;
		}

		private void SampleTransform()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				m_lastPosition = transform.position;
				m_lastScale = transform.lossyScale;
				m_lastRotation = transform.rotation;
			}
			else
			{
				m_lastPosition = m_trs.position;
				m_lastScale = m_trs.lossyScale;
				m_lastRotation = m_trs.rotation;
			}
#else
            _lastPosition = _trs.position;
            _lastScale = _trs.lossyScale;
            _lastRotation = _trs.rotation;
#endif
		}

		private void Run()
		{
			if (TransformChanged())
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					EditorUtility.SetDirty(this);
					for (var i = 0; i < m_connections.Length; i++)
					{
						EditorUtility.SetDirty(m_connections[i].spline);
					}
				}
#endif
				UpdateConnectedComputers();
				SampleTransform();
			}
		}

		public SplinePoint GetPoint(int connectionIndex, bool swapTangents)
		{
			SplinePoint point = PointToWorld(m_connections[connectionIndex].point);
			if (m_connections[connectionIndex].invertTangents && swapTangents)
			{
				Vector3 tempTan = point.tangent;
				point.tangent = point.tangent2;
				point.tangent2 = tempTan;
			}

			return point;
		}

		public void SetPoint(int connectionIndex, SplinePoint worldPoint, bool swappedTangents)
		{
			Connection connection = m_connections[connectionIndex];
			connection.point = PointToLocal(worldPoint);
			if (connection.invertTangents && swappedTangents)
			{
				Vector3 tempTan = connection.point.tangent;
				connection.point.tangent = connection.point.tangent2;
				connection.point.tangent2 = tempTan;
			}

			if (type == Type.Smooth)
			{
				if (connection.point.type == SplinePoint.Type.SmoothFree)
				{
					for (var i = 0; i < m_connections.Length; i++)
					{
						if (i == connectionIndex)
						{
							continue;
						}

						Vector3 tanDir = (connection.point.tangent - connection.point.position).normalized;
						if (tanDir == Vector3.zero)
						{
							tanDir = -(connection.point.tangent2 - connection.point.position).normalized;
						}

						float tan1Length = (m_connections[i].point.tangent - m_connections[i].point.position).magnitude;
						float tan2Length = (m_connections[i].point.tangent2 - m_connections[i].point.position)
							.magnitude;
						m_connections[i].point = connection.point;
						m_connections[i].point.tangent = m_connections[i].point.position + tanDir * tan1Length;
						m_connections[i].point.tangent2 = m_connections[i].point.position - tanDir * tan2Length;
					}
				}
				else
				{
					for (var i = 0; i < m_connections.Length; i++)
					{
						if (i == connectionIndex)
						{
							continue;
						}

						m_connections[i].point = connection.point;
					}
				}
			}
		}

		public void ClearConnections()
		{
			for (int i = m_connections.Length - 1; i >= 0; i--)
			{
				if (m_connections[i].spline != null)
				{
					m_connections[i].spline.DisconnectNode(m_connections[i].pointIndex);
				}
			}

			m_connections = new Connection[0];
		}

		public void UpdateConnectedComputers(SplineComputer excludeComputer = null)
		{
			for (int i = m_connections.Length - 1; i >= 0; i--)
			{
				if (!m_connections[i].isValid)
				{
					RemoveConnection(i);
					continue;
				}

				if (m_connections[i].spline == excludeComputer)
				{
					continue;
				}

				if (type == Type.Smooth && i != 0)
				{
					SetPoint(i, GetPoint(0, false), false);
				}

				SplinePoint point = GetPoint(i, true);
				if (!transformNormals)
				{
					point.normal = m_connections[i].spline.GetPointNormal(m_connections[i].pointIndex);
				}

				if (!transformTangents)
				{
					point.tangent = m_connections[i].spline.GetPointTangent(m_connections[i].pointIndex);
					point.tangent2 = m_connections[i].spline.GetPointTangent2(m_connections[i].pointIndex);
				}

				if (!transformSize)
				{
					point.size = m_connections[i].spline.GetPointSize(m_connections[i].pointIndex);
				}

				m_connections[i].spline.SetPoint(m_connections[i].pointIndex, point);
			}
		}

		public void UpdatePoint(SplineComputer computer, int pointIndex, SplinePoint point, bool updatePosition = true)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				transform.position = point.position;
			}
			else
			{
				m_trs.position = point.position;
			}
#else
            _trs.position = point.position;
#endif
			for (var i = 0; i < m_connections.Length; i++)
			{
				if (m_connections[i].spline == computer && m_connections[i].pointIndex == pointIndex)
				{
					SetPoint(i, point, true);
				}
			}
		}

		public void UpdatePoints()
		{
			for (int i = m_connections.Length - 1; i >= 0; i--)
			{
				if (!m_connections[i].isValid)
				{
					RemoveConnection(i);
					continue;
				}

				SplinePoint point = m_connections[i].spline.GetPoint(m_connections[i].pointIndex);
				point.SetPosition(transform.position);
				SetPoint(i, point, true);
			}
		}

#if UNITY_EDITOR
		//Use this to maintain the connections between computers in the editor
		public void EditorMaintainConnections()
		{
			RemoveInvalidConnections();
		}
#endif
		//Remove invalid connections
		protected void RemoveInvalidConnections()
		{
			for (int i = m_connections.Length - 1; i >= 0; i--)
			{
				if (m_connections[i] == null || !m_connections[i].isValid)
				{
					RemoveConnection(i);
				}
			}
		}

		public virtual void AddConnection(SplineComputer computer, int pointIndex)
		{
			RemoveInvalidConnections();
			Node connected = computer.GetNode(pointIndex);
			if (connected != null)
			{
				Debug.LogError(computer.name + " is already connected to node " + connected.name + " at point " +
				               pointIndex);
				return;
			}

			SplinePoint point = computer.GetPoint(pointIndex);
			point.SetPosition(transform.position);
			ArrayUtility.Add(ref m_connections, new Connection(computer, pointIndex, PointToLocal(point)));
			if (m_connections.Length == 1)
			{
				SetPoint(m_connections.Length - 1, point, true);
			}

			UpdateConnectedComputers();
		}

		protected SplinePoint PointToLocal(SplinePoint worldPoint)
		{
			worldPoint.position = Vector3.zero;
			worldPoint.tangent = transform.InverseTransformPoint(worldPoint.tangent);
			worldPoint.tangent2 = transform.InverseTransformPoint(worldPoint.tangent2);
			worldPoint.normal = transform.InverseTransformDirection(worldPoint.normal);
			worldPoint.size /= (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
			return worldPoint;
		}

		protected SplinePoint PointToWorld(SplinePoint localPoint)
		{
			localPoint.position = transform.position;
			localPoint.tangent = transform.TransformPoint(localPoint.tangent);
			localPoint.tangent2 = transform.TransformPoint(localPoint.tangent2);
			localPoint.normal = transform.TransformDirection(localPoint.normal);
			localPoint.size *= (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
			return localPoint;
		}

		public virtual void RemoveConnection(SplineComputer computer, int pointIndex)
		{
			int index = -1;
			for (var i = 0; i < m_connections.Length; i++)
			{
				if (m_connections[i].pointIndex == pointIndex && m_connections[i].spline == computer)
				{
					index = i;
					break;
				}
			}

			if (index < 0)
			{
				return;
			}

			RemoveConnection(index);
		}

		private void RemoveConnection(int index)
		{
			var newConnections = new Connection[m_connections.Length - 1];
			SplineComputer spline = m_connections[index].spline;
			int pointIndex = m_connections[index].pointIndex;
			for (var i = 0; i < m_connections.Length; i++)
			{
				if (i < index)
				{
					newConnections[i] = m_connections[i];
				}
				else if (i == index)
				{
					continue;
				}
				else
				{
					newConnections[i - 1] = m_connections[i];
				}
			}

			m_connections = newConnections;
		}

		public virtual bool HasConnection(SplineComputer computer, int pointIndex)
		{
			for (int i = m_connections.Length - 1; i >= 0; i--)
			{
				if (!m_connections[i].isValid)
				{
					RemoveConnection(i);
					continue;
				}

				if (m_connections[i].spline == computer && m_connections[i].pointIndex == pointIndex)
				{
					return true;
				}
			}

			return false;
		}

		public Connection[] GetConnections()
		{
			return m_connections;
		}

		[Serializable]
		public class Connection
		{
			public bool invertTangents;

			[SerializeField]
			private int m_pointIndex;

			[SerializeField]
			private SplineComputer m_computer;

			[SerializeField]
			[HideInInspector]
			internal SplinePoint point;

			internal Connection(SplineComputer comp, int index, SplinePoint inputPoint)
			{
				m_pointIndex = index;
				m_computer = comp;
				point = inputPoint;
			}

			public SplineComputer spline => m_computer;

			public int pointIndex => m_pointIndex;

			internal bool isValid
			{
				get
				{
					if (m_computer == null)
					{
						return false;
					}

					if (m_pointIndex >= m_computer.pointCount)
					{
						return false;
					}

					return true;
				}
			}
		}
	}
}
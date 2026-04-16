using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Node Connector")]
    public class Node : MonoBehaviour
    {
        [System.Serializable]
        public class Connection
        {
            public SplineComputer spline
            {
                get { return m_computer; }
            }

            public int pointIndex
            {
                get { return m_pointIndex; }
            }

            public bool invertTangents = false;

            [SerializeField]
            private int m_pointIndex = 0;
            [SerializeField]
            private SplineComputer m_computer = null;
            [SerializeField]
            [HideInInspector]
            internal SplinePoint point;

            internal bool isValid
            {
                get
                {
                    if (m_computer == null) return false;
                    if (m_pointIndex >= m_computer.pointCount) return false;
                    return true;
                }
            }

            internal Connection(SplineComputer comp, int index, SplinePoint inputPoint)
            {
                m_pointIndex = index;
                m_computer = comp;
                point = inputPoint;
            }
        }
        public enum Type { Smooth, Free }
        [HideInInspector]
        public Type type = Type.Smooth;

        public bool transformNormals
        {
            get { return m_transformNormals; }
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
            get { return m_transformSize; }
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
            get { return m_transformTangents; }
            set
            {
                if (value != m_transformTangents)
                {
                    m_transformTangents = value;
                    UpdatePoints();
                }
            }
        }

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

        private void Awake()
        {
            m_trs = transform;
            SampleTransform();
        }


        void LateUpdate()
        {
            Run();
        }

        void Update()
        {
            Run();
        }

        bool TransformChanged()
        {
#if UNITY_EDITOR
            if(m_trs == null) return m_lastPosition != transform.position || m_lastRotation != transform.rotation || m_lastScale != transform.lossyScale;
#endif
            return m_lastPosition != m_trs.position || m_lastRotation != m_trs.rotation || m_lastScale != m_trs.lossyScale;
        }

        void SampleTransform() {
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
            return;
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
                    UnityEditor.EditorUtility.SetDirty(this);
                    for (int i = 0; i < m_connections.Length; i++)
                    {
                        UnityEditor.EditorUtility.SetDirty(m_connections[i].spline);
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
                    for (int i = 0; i < m_connections.Length; i++)
                    {
                        if (i == connectionIndex) continue;
                        Vector3 tanDir = (connection.point.tangent - connection.point.position).normalized;
                        if (tanDir == Vector3.zero) tanDir = -(connection.point.tangent2 - connection.point.position).normalized;
                        float tan1Length = (m_connections[i].point.tangent - m_connections[i].point.position).magnitude;
                        float tan2Length = (m_connections[i].point.tangent2 - m_connections[i].point.position).magnitude;
                        m_connections[i].point = connection.point;
                        m_connections[i].point.tangent = m_connections[i].point.position + tanDir * tan1Length;
                        m_connections[i].point.tangent2 = m_connections[i].point.position - tanDir * tan2Length;
                    }
                }
                else
                {
                    for (int i = 0; i < m_connections.Length; i++)
                    {
                        if (i == connectionIndex) continue;
                        m_connections[i].point = connection.point;
                    }
                }
            }
        }

        void OnDestroy()
        {
            ClearConnections();
        }

        public void ClearConnections()
        {
            for (int i = m_connections.Length-1; i >= 0; i--)
            {
                if (m_connections[i].spline != null) m_connections[i].spline.DisconnectNode(m_connections[i].pointIndex);
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

                if (m_connections[i].spline == excludeComputer) continue;

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
            for (int i = 0; i < m_connections.Length; i++)
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
                if (m_connections[i] == null || !m_connections[i].isValid) RemoveConnection(i);
            }
        }

        public virtual void AddConnection(SplineComputer computer, int pointIndex)
        {
            RemoveInvalidConnections();
            Node connected = computer.GetNode(pointIndex);
            if (connected != null)
            {
                Debug.LogError(computer.name + " is already connected to node " + connected.name + " at point " + pointIndex);
                return;
            }
            SplinePoint point = computer.GetPoint(pointIndex);
            point.SetPosition(transform.position);
            ArrayUtility.Add(ref m_connections, new Connection(computer, pointIndex, PointToLocal(point)));
            if(m_connections.Length == 1) SetPoint(m_connections.Length - 1, point, true);
            UpdateConnectedComputers();
        }

        protected SplinePoint PointToLocal(SplinePoint worldPoint)
        {
            worldPoint.position = Vector3.zero;
            worldPoint.tangent = transform.InverseTransformPoint(worldPoint.tangent);
            worldPoint.tangent2 = transform.InverseTransformPoint(worldPoint.tangent2);
            worldPoint.normal = transform.InverseTransformDirection(worldPoint.normal);
            worldPoint.size /= (transform.localScale.x + transform.localScale.y + transform.localScale.z)/ 3f;
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
            for (int i = 0; i < m_connections.Length; i++)
            {
                if (m_connections[i].pointIndex == pointIndex && m_connections[i].spline == computer)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0) return;
            RemoveConnection(index);
        }

        private void RemoveConnection(int index)
        {
            Connection[] newConnections = new Connection[m_connections.Length - 1];
            SplineComputer spline = m_connections[index].spline;
            int pointIndex = m_connections[index].pointIndex;
            for (int i = 0; i < m_connections.Length; i++)
            {
                if (i < index) newConnections[i] = m_connections[i];
                else if (i == index) continue;
                else newConnections[i - 1] = m_connections[i];
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
                if (m_connections[i].spline == computer && m_connections[i].pointIndex == pointIndex) return true;
            }
            return false;
        }

        public Connection[] GetConnections()
        {
            return m_connections;
        }

    }
}

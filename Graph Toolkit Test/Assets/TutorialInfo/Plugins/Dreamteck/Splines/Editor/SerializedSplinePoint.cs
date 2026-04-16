namespace Dreamteck.Splines.Editor
{
    using UnityEditor;
    using UnityEngine;

    public struct SerializedSplinePoint
    {
        public bool changed;

        public SplinePoint.Type type
        {
            get
            {
                return (SplinePoint.Type)m_type.enumValueIndex;
            }
            set
            {
                if (value != type)
                {
                    m_type.enumValueIndex = (int)value;
                    changed = true;
                }
            }
        }

        public Vector3 position
        {
            get { return m_position.vector3Value; }
            set
            {
                if (value != position)
                {
                    m_position.vector3Value = value;
                    changed = true;
                }
            }
        }

        public Vector3 tangent
        {
            get { return m_tangent.vector3Value; }
            set
            {
                if (value != tangent)
                {
                    m_tangent.vector3Value = value;
                    changed = true;
                }
            }
        }
        public Vector3 tangent2
        {
            get { return m_tangent2.vector3Value; }
            set
            {
                if (value != tangent2)
                {
                    m_tangent2.vector3Value = value;
                    changed = true;
                }
            }
        }

        public Color color
        {
            get { return m_color.colorValue; }
            set
            {
                if (value != color)
                {
                    m_color.colorValue = value;
                    changed = true;
                }
            }
        }

        public Vector3 normal
        {
            get { return m_normal.vector3Value; }
            set
            {
                if (value != normal)
                {
                    m_normal.vector3Value = value;
                    changed = true;
                }
            }
        }
        public float size
        {
            get { return m_size.floatValue; }
            set
            {
                if (value != size)
                {
                    m_size.floatValue = value;
                    changed = true;
                }
            }
        }


        private SerializedProperty m_point;
        private SerializedProperty m_position;
        private SerializedProperty m_tangent;
        private SerializedProperty m_tangent2;
        private SerializedProperty m_normal;
        private SerializedProperty m_size;
        private SerializedProperty m_color;
        private SerializedProperty m_type;


        public SerializedSplinePoint(SerializedProperty input)
        {
            m_point = input;
            m_position = m_point.FindPropertyRelative("position");
            m_tangent = m_point.FindPropertyRelative("tangent");
            m_tangent2 = m_point.FindPropertyRelative("tangent2");
            m_normal = m_point.FindPropertyRelative("normal");
            m_size = m_point.FindPropertyRelative("size");
            m_color = m_point.FindPropertyRelative("color");
            m_type = m_point.FindPropertyRelative("_type");
            changed = false;
        }

        public void SetPoint(SplinePoint point)
        {
            CheckForChange(point);
            position = point.position;
            tangent = point.tangent;
            tangent2 = point.tangent2;
            normal = point.normal;
            size = point.size;
            color = point.color;
            type = point.type;
        }

        private void CheckForChange(SplinePoint point)
        {
            if (position != point.position)
            {
                changed = true;
                return;
            }

            if (tangent != point.tangent)
            {
                changed = true;
                return;
            }

            if (tangent2 != point.tangent2)
            {
                changed = true;
                return;
            }

            if (normal != point.normal)
            {
                changed = true;
                return;
            }

            if (size != point.size)
            {
                changed = true;
                return;
            }

            if (color != point.color)
            {
                changed = true;
                return;
            }

            if (type != point.type)
            {
                changed = true;
                return;
            }
        }

        public void CopyFrom(SerializedSplinePoint point)
        {
            position = point.position;
            tangent = point.tangent;
            tangent2 = point.tangent2;
            normal = point.normal;
            size = point.size;
            color = point.color;
            type = point.type;
        }

        public SplinePoint CreateSplinePoint()
        {
            SplinePoint point = new SplinePoint();
            point.type = type;
            point.position = position;
            point.tangent = tangent;
            point.tangent2 = tangent2;
            point.normal = normal;
            point.size = size;
            point.color = color;
            point.isDirty = changed;
            return point;
        }

        public void SetPosition(Vector3 pos)
        {
            tangent -= position - pos;
            tangent2 -= position - pos;
            position = pos;
        }

        public void SetTangentPosition(Vector3 pos)
        {
            tangent = pos;
            switch ((SplinePoint.Type)m_type.enumValueIndex)
            {
                case SplinePoint.Type.SmoothMirrored: SmoothMirrorTangent2(); break;
                case SplinePoint.Type.SmoothFree: SmoothFreeTangent2(); break;
            }
        }

        public void SetTangent2Position(Vector3 pos)
        {
            tangent2 = pos;
            switch ((SplinePoint.Type)m_type.enumValueIndex)
            {
                case SplinePoint.Type.SmoothMirrored: SmoothMirrorTangent(); break;
                case SplinePoint.Type.SmoothFree: SmoothFreeTangent(); break;
            }
        }

        private void SmoothMirrorTangent2()
        {
            tangent2 = position + (position - tangent);
        }

        private void SmoothMirrorTangent()
        {
            tangent = position + (position - tangent2);
        }

        private void SmoothFreeTangent2()
        {
            tangent2 = position + (position - tangent).normalized * (tangent2 - position).magnitude;
        }

        private void SmoothFreeTangent()
        {
            tangent = position + (position - tangent2).normalized * (tangent - position).magnitude;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives {
    public class SplinePrimitive
    {
        protected bool m_closed = false;
        protected SplinePoint[] m_points = new SplinePoint[0];

        public Vector3 offset = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public bool is2D = false;

        public virtual void Calculate()
        {
            Generate();
            ApplyOffset();
        }

        protected virtual void Generate()
        {
        
        }

        public Spline CreateSpline()
        {
            Generate();
            ApplyOffset();
            Spline spline = new Spline(GetSplineType());
            spline.points = m_points;
            if (m_closed) spline.Close();
            return spline;
        }

        public void UpdateSpline(Spline spline)
        {
            Generate();
            ApplyOffset();
            spline.type = GetSplineType();
            spline.points = m_points;
            if (m_closed) spline.Close();
            else if (spline.isClosed) spline.Break();
        }

        public SplineComputer CreateSplineComputer(string name, Vector3 position, Quaternion rotation)
        {
            Generate();
            ApplyOffset();
            GameObject go = new GameObject(name);
            SplineComputer comp = go.AddComponent<SplineComputer>();
            comp.SetPoints(m_points, SplineComputer.Space.Local);
            if (m_closed) comp.Close();
            comp.transform.position = position;
            comp.transform.rotation = rotation;
            return comp;
        }

        public void UpdateSplineComputer(SplineComputer comp)
        {
            Generate();
            ApplyOffset();
            comp.type = GetSplineType();
            comp.SetPoints(m_points, SplineComputer.Space.Local);
            if (m_closed) comp.Close();
            else if (comp.isClosed) comp.Break();
        }

        public SplinePoint[] GetPoints()
        {
            return m_points;
        }

        public virtual Spline.Type GetSplineType()
        {
            return Spline.Type.CatmullRom;
        }

        public bool GetIsClosed()
        {
            return m_closed;
        }

        void ApplyOffset()
        {
            Quaternion freeRot = Quaternion.Euler(rotation);
            if (is2D) freeRot = Quaternion.AngleAxis(-rotation.z, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);
            for (int i = 0; i < m_points.Length; i++)
            {
                m_points[i].position = freeRot * m_points[i].position;
                m_points[i].tangent = freeRot *  m_points[i].tangent;
                m_points[i].tangent2 = freeRot * m_points[i].tangent2;
                m_points[i].normal = freeRot * m_points[i].normal;
            }
            for (int i = 0; i < m_points.Length; i++) m_points[i].SetPosition(m_points[i].position + offset);
        }

        protected void CreatePoints(int count, SplinePoint.Type type)
        {
            if (m_points.Length != count) m_points = new SplinePoint[count];
            for (int i = 0; i < m_points.Length; i++)
            {
                m_points[i].type = type;
                m_points[i].normal = Vector3.up;
                m_points[i].color = Color.white;
                m_points[i].size = 1f;
            }
        }
    }
}

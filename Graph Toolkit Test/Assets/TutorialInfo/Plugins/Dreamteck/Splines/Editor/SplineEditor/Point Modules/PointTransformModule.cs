namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PointTransformModule : PointModule
    {
        public enum EditSpace { World, Transform, Spline }
        public EditSpace editSpace = EditSpace.World;
        public Vector3 scale = Vector3.one, offset = Vector3.zero;
        protected Quaternion m_rotation = Quaternion.identity;
        protected Vector3 m_origin = Vector3.zero;
        protected SplinePoint[] m_originalPoints = new SplinePoint[0];
        protected SplinePoint[] m_localPoints = new SplinePoint[0];

        private Matrix4x4 m_matrix = new Matrix4x4();
        private Matrix4x4 m_inverseMatrix = new Matrix4x4();
        private bool m_unapplied = true;
        SplineSample m_evalResult = new SplineSample();

        public PointTransformModule(SplineEditor editor) : base(editor)
        {

        }

        public override void Reset()
        {
            base.Reset();
            GetRotation();
            m_origin = selectionCenter;
            scale = Vector3.one;
            m_matrix.SetTRS(m_origin, m_rotation, Vector3.one);
            m_inverseMatrix = m_matrix.inverse;
            m_localPoints = m_editor.GetPointsArray();
            for (int i = 0; i < m_localPoints.Length; i++) InverseTransformPoint(ref m_localPoints[i]);
        }

        protected void GetRotation()
        {
            switch (editSpace)
            {
                case EditSpace.World: m_rotation = Quaternion.identity; break;
                case EditSpace.Transform: m_rotation = TransformUtility.GetRotation(m_editor.matrix); break;
                case EditSpace.Spline:
                    if (m_editor.evaluate == null)
                    {
                        Debug.LogError("Unassigned handler evaluate for Spline Editor.");
                        break;
                    }
                    if (selectedPoints.Count == 1)
                    {
                        m_editor.evaluate((double)selectedPoints[0] / (points.Length - 1), ref m_evalResult);
                        m_rotation = m_evalResult.rotation;
                    }
                    else m_rotation = Quaternion.identity;
                    break;
            }
        }

        public override void LoadState()
        {
            base.LoadState();
            editSpace = (EditSpace)LoadInt("editSpace");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("editSpace", (int)editSpace);
        }

        protected void SetDirty()
        {
            RegisterChange();
            m_unapplied = true;
        }

        protected bool IsDirty()
        {
            return m_unapplied;
        }

        public virtual void Revert()
        {
            m_editor.SetPointsArray(m_originalPoints);
            m_editor.ApplyModifiedProperties();
            m_unapplied = false;
        }

        public virtual void Apply()
        {
            RegisterChange();
            CacheOriginalPoints();
            m_unapplied = false;
        }

        public override void Select()
        {
            base.Select();
            CacheOriginalPoints();
        }

        public override void Deselect()
        {
            base.Deselect();
            m_unapplied = false;
        }

        private void CacheOriginalPoints()
        {
            m_originalPoints = m_editor.GetPointsArray();
        }

        protected void PrepareTransform()
        {
            m_matrix.SetTRS(m_origin + offset, m_rotation, scale);
        }

        protected Vector3 TransformPosition(Vector3 position)
        {
            return m_matrix.MultiplyPoint3x4(position);
        }

        protected Vector3 InverseTransformPosition(Vector3 position)
        {
            return m_inverseMatrix.MultiplyPoint3x4(position);
        }

        protected Vector3 TransformDirection(Vector3 direction)
        {
            return m_matrix.MultiplyVector(direction);
        }

        protected Vector3 InverseTransformDirection(Vector3 direction)
        {
            return m_inverseMatrix.MultiplyVector(direction);
        }

        protected void TransformPoint(ref SplinePoint point, bool normals = true, bool tangents = true, bool size = false)
        {
            if (tangents)
            {
                point.position = TransformPosition(point.position);
                point.tangent = TransformPosition(point.tangent);
                point.tangent2 = TransformPosition(point.tangent2);
            }
            else
            {
                point.SetPosition(TransformPosition(point.position));
            }
            if(normals) point.normal = TransformDirection(point.normal).normalized;
            if (size)
            {
                float avg = (scale.x + scale.y + scale.z) / 3f;
                point.size *= avg;
            }
        }

        protected void InverseTransformPoint(ref SplinePoint point, bool normals = true, bool tangents = true, bool size = false)
        {
            if (tangents)
            {
                point.position = InverseTransformPosition(point.position);
                point.tangent = InverseTransformPosition(point.tangent);
                point.tangent2 = InverseTransformPosition(point.tangent2);
            } else point.SetPosition(TransformPosition(point.position));

            if (normals) point.normal = InverseTransformDirection(point.normal).normalized;
            if (size)
            {
                float avg = (scale.x + scale.y + scale.z) / 3f;
                point.size /= avg;
            }
        }
    }
}

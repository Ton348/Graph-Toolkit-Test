using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Projector")]
    public class SplineProjector : SplineTracer
    {
        public enum Mode {Accurate, Cached}
        public Mode mode
        {
            get { return m_mode; }
            set
            {
                if(value != m_mode)
                {
                    m_mode = value;
                    Rebuild();
                }
            }
        }

        public bool autoProject
        {
            get { return m_autoProject; }
            set
            {
                if(value != m_autoProject)
                {
                    m_autoProject = value;
                    if (m_autoProject) Rebuild();
                }
            }
        }

        public int subdivide
        {
            get { return m_subdivide; }
            set
            {
                if (value != m_subdivide)
                {
                    m_subdivide = value;
                    if (m_mode == Mode.Accurate) Rebuild();
                }
            }
        }

        public Transform projectTarget
        {
            get {
                if (m_projectTarget == null) return transform;
                return m_projectTarget; 
            }
            set
            {
                if (value != m_projectTarget)
                {
                    m_projectTarget = value;
                    Rebuild();
                }
            }
        }

        public GameObject targetObject
        {
            get
            {
                if (m_targetObject == null)
                {
                    if (m_applyTarget != null) //Temporary check to migrate SplineProjectors that use target
                    {
                        m_targetObject = m_applyTarget.gameObject;
                        m_applyTarget = null;
                        return m_targetObject;
                    }
                }
                return m_targetObject;
            }

            set
            {
                if (value != m_targetObject)
                {
                    m_targetObject = value;
                    RefreshTargets();
                    Rebuild();
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Mode m_mode = Mode.Cached;
        [SerializeField]
        [HideInInspector]
        private bool m_autoProject = true;
        [SerializeField]
        [HideInInspector]
        [Range(3, 8)]
        private int m_subdivide = 4;
        [SerializeField]
        [HideInInspector]
        private Transform m_projectTarget;


        [SerializeField]
        [HideInInspector]
        private Transform m_applyTarget = null;
        [SerializeField]
        [HideInInspector]
        private GameObject m_targetObject;

        [SerializeField]
        [HideInInspector]
        public Vector2 offset;
        [SerializeField]
        [HideInInspector]
        public Vector3 rotationOffset = Vector3.zero;

        public event SplineReachHandler onEndReached;
        public event SplineReachHandler onBeginningReached;

        [SerializeField]
        [HideInInspector]
        Vector3 m_lastPosition = Vector3.zero;

        protected override void Reset()
        {
            base.Reset();
            m_projectTarget = transform;
        }

        protected override Transform GetTransform()
        {
            if (targetObject == null) return null;
            return targetObject.transform;
        }

        protected override Rigidbody GetRigidbody()
        {
            if (targetObject == null) return null;
            return targetObject.GetComponent<Rigidbody>();
        }

        protected override Rigidbody2D GetRigidbody2D()
        {
            if (targetObject == null) return null;
            return targetObject.GetComponent<Rigidbody2D>();
        }


        protected override void LateRun()
        {
            base.LateRun();
            if (autoProject)
            {
                if (projectTarget && m_lastPosition != projectTarget.position)
                {
                    m_lastPosition = projectTarget.position;
                    CalculateProjection();
                }
            }
         }

        protected override void PostBuild()
        {
            base.PostBuild();
            CalculateProjection();
        }

        protected override void OnSplineChanged()
        {
            if (spline != null)
            {
                if (m_mode == Mode.Accurate)
                {
                    spline.Project(m_projectTarget.position, ref m_result, clipFrom, clipTo, SplineComputer.EvaluateMode.Calculate, subdivide);
                } 
                else
                {
                    spline.Project(m_projectTarget.position, ref m_result, clipFrom, clipTo);
                }
                m_result.percent = ClipPercent(m_result.percent);
            }
        }


        private void Project()
        {
            if (m_mode == Mode.Accurate && spline != null)
            {
                spline.Project(m_projectTarget.position, ref m_result, clipFrom, clipTo, SplineComputer.EvaluateMode.Calculate, subdivide);
                m_result.percent = ClipPercent(m_result.percent);
            }
            else
            {
                Project(m_projectTarget.position, ref m_result);
            }
        }

        public void CalculateProjection()
        {
            if (m_projectTarget == null) return;
            double lastPercent = m_result.percent;
            Project();

            if (onBeginningReached != null && m_result.percent <= clipFrom)
            {
                if (!Mathf.Approximately((float)lastPercent, (float)m_result.percent))
                {
                    onBeginningReached();
                    if (samplesAreLooped)
                    {
                        CheckTriggers(lastPercent, 0.0);
                        CheckNodes(lastPercent, 0.0);
                        lastPercent = 1.0;
                    }
                }
            }
            else if (onEndReached != null && m_result.percent >= clipTo)
            {
                if (!Mathf.Approximately((float)lastPercent, (float)m_result.percent))
                {
                    onEndReached();
                    if (samplesAreLooped)
                    {
                        CheckTriggers(lastPercent, 1.0);
                        CheckNodes(lastPercent, 1.0);
                        lastPercent = 0.0;
                    }
                }
            }

            CheckTriggers(lastPercent, m_result.percent);
            CheckNodes(lastPercent, m_result.percent);
            

            if (targetObject != null)
            {
                ApplyMotion();
            }

            InvokeTriggers();
            InvokeNodes();
            m_lastPosition = projectTarget.position;
        }
    }
}

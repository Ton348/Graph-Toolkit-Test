using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    public class SplineTracer : SplineUser
    {
        public class NodeConnection
        {
            public Node node;
            public int point = 0;

            public NodeConnection(Node node, int point)
            {
                this.node = node;
                this.point = point;
            }
        }

        public enum PhysicsMode { Transform, Rigidbody, Rigidbody2D }
        public PhysicsMode physicsMode
        {
            get { return m_physicsMode; }
            set
            {
                m_physicsMode = value;
                RefreshTargets();
            }
        }

        public TransformModule motion
        {
            get
            {
                if (m_motion == null) m_motion = new TransformModule();
                return m_motion;
            }
        }

        /// <summary>
        /// Returns the unmodified result from the evaluation
        /// </summary>
        public SplineSample result
        {
            get { return m_result; }
        }

        public bool dontLerpDirection
        {
            get { return m_dontLerpDirection; }
            set
            {
                if (value != m_dontLerpDirection)
                {
                    m_dontLerpDirection = value;
                    ApplyMotion();
                }
            }
        }

        public virtual Spline.Direction direction
        {
            get { return m_direction; }
            set
            {
                if (value != m_direction)
                {
                    m_direction = value;
                    ApplyMotion();
                }
            }
        }

        [HideInInspector]
        public bool applyDirectionRotation = true;
        [HideInInspector]
        public bool useTriggers = false;
        [HideInInspector]
        public int triggerGroup = 0;
        [SerializeField]
        [HideInInspector]
        protected Spline.Direction m_direction = Spline.Direction.Forward;

        [SerializeField]
        [HideInInspector]
        protected bool m_dontLerpDirection = false;

        [SerializeField]
        [HideInInspector]
        protected PhysicsMode m_physicsMode = PhysicsMode.Transform;
        [SerializeField]
        [HideInInspector]
        protected TransformModule m_motion = null;


        [SerializeField]
        [HideInInspector]
        protected Rigidbody m_targetRigidbody = null;
        [SerializeField]
        [HideInInspector]
        protected Rigidbody2D m_targetRigidbody2D = null;
        [SerializeField]
        [HideInInspector]
        protected Transform m_targetTransform = null;
        [SerializeField]
        [HideInInspector]
        protected SplineSample m_result = new SplineSample();

        public delegate void JunctionHandler(List<NodeConnection> passed);

        public event JunctionHandler onNode;
        public event EmptySplineHandler onMotionApplied;

        private SplineTrigger[] m_triggerInvokeQueue = new SplineTrigger[0];
        private List<NodeConnection> m_nodeConnectionQueue = new List<NodeConnection>();
        private int m_addTriggerIndex = 0;

        private const double s_mInDelta = 0.000001;

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            RefreshTargets();
            ApplyMotion();
        }
#endif 

        protected override void Awake()
        {
            base.Awake();
            RefreshTargets();
        }

        protected virtual void Start()
        {

        }

        public virtual void SetPercent(double percent, bool checkTriggers = false, bool handleJunctions = false)
        {
            if (sampleCount == 0) return;
            double lastPercent = m_result.percent;
            Evaluate(percent, ref m_result);
            ApplyMotion();
            if (checkTriggers)
            {
                CheckTriggers(lastPercent, percent);
                InvokeTriggers();
            }
            if (handleJunctions)
            {
                CheckNodes(lastPercent, percent);
            }
        }

        public double GetPercent()
        {
            return m_result.percent;
        }

        public virtual void SetDistance(float distance, bool checkTriggers = false, bool handleJunctions = false)
        {
            double lastPercent = m_result.percent;
            Evaluate(Travel(0.0, distance, Spline.Direction.Forward), ref m_result);
            ApplyMotion();
            if (checkTriggers)
            {
                CheckTriggers(lastPercent, m_result.percent);
                InvokeTriggers();
            }
            if (handleJunctions)
            {
                CheckNodes(lastPercent, m_result.percent);
            }
        }

        protected virtual Rigidbody GetRigidbody()
        {
            return GetComponent<Rigidbody>();
        }

        protected virtual Rigidbody2D GetRigidbody2D()
        {
            return GetComponent<Rigidbody2D>();
        }

        protected virtual Transform GetTransform()
        {
            return transform;
        }

        protected void ApplyMotion()
        {
            if (sampleCount == 0) return;
            if (m_dontLerpDirection)
            {
                double unclippedPercent = UnclipPercent(m_result.percent);
                int index;
                double lerp;
                spline.GetSamplingValues(unclippedPercent, out index, out lerp);
                m_result.forward = spline[index].forward;
                m_result.up = spline[index].up;
            }

            motion.targetUser = this;
            motion.splineResult = m_result;
            if (applyDirectionRotation) motion.direction = m_direction;
            else motion.direction = Spline.Direction.Forward;

            switch (m_physicsMode)
            {
                case PhysicsMode.Transform:
                    if (m_targetTransform == null) RefreshTargets();
                    if (m_targetTransform == null) return;
                    motion.ApplyTransform(m_targetTransform);
                    if (onMotionApplied != null) onMotionApplied();
                    break;
                case PhysicsMode.Rigidbody:
                    if (m_targetRigidbody == null)
                    {
                        RefreshTargets();
                        if (m_targetRigidbody == null)  throw new MissingComponentException("There is no Rigidbody attached to " + name + " but the Physics mode is set to use one.");
                    }
                    motion.ApplyRigidbody(m_targetRigidbody);
                    if (onMotionApplied != null) onMotionApplied();
                    break;
                case PhysicsMode.Rigidbody2D:
                    if (m_targetRigidbody2D == null)
                    {
                        RefreshTargets();
                        if (m_targetRigidbody2D == null) throw new MissingComponentException("There is no Rigidbody2D attached to " + name + " but the Physics mode is set to use one.");
                    }
                    motion.ApplyRigidbody2D(m_targetRigidbody2D);
                    if (onMotionApplied != null) onMotionApplied();
                    break;
            }
        }

        protected void CheckNodes(double from, double to)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (onNode == null) return;
            if (from == to) return;
            UnclipPercent(ref from);
            UnclipPercent(ref to);
            Spline.FormatFromTo(ref from, ref to, true);
            int fromPoint, toPoint;
            fromPoint = spline.PercentToPointIndex(from, m_direction);
            toPoint = spline.PercentToPointIndex(to, m_direction);

            if (fromPoint != toPoint)
            {
                if (m_direction == Spline.Direction.Forward)
                {
                    for (int i = fromPoint + 1; i <= toPoint; i++)
                    {
                        NodeConnection junction = GetJunction(i);
                        if (junction != null) m_nodeConnectionQueue.Add(junction);
                    }
                }
                else
                {
                    for (int i = toPoint - 1; i >= fromPoint; i--)
                    {
                        NodeConnection junction = GetJunction(i);
                        if (junction != null) m_nodeConnectionQueue.Add(junction);
                    }
                }
            }
            else if (from < s_mInDelta && to > from)
            {
                NodeConnection junction = GetJunction(0);
                if (junction != null) m_nodeConnectionQueue.Add(junction);
            }
            else if (to > 1.0 - s_mInDelta && from < to)
            {
                int pointCount = spline.pointCount - 1;
                if (spline.isClosed)
                {
                    pointCount = spline.pointCount;
                }
                NodeConnection junction = GetJunction(pointCount);
                if (junction != null) m_nodeConnectionQueue.Add(junction);
            }
        }

        protected void InvokeNodes()
        {
            if(m_nodeConnectionQueue.Count > 0)
            {
                onNode(m_nodeConnectionQueue);
                m_nodeConnectionQueue.Clear();
            }
        }

        protected void CheckTriggers(double from, double to)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!useTriggers) return;
            if (from == to) return;
            UnclipPercent(ref from);
            UnclipPercent(ref to);
            if (triggerGroup < 0 || triggerGroup >= spline.triggerGroups.Length) return;
            for (int i = 0; i < spline.triggerGroups[triggerGroup].triggers.Length; i++)
            {
                if (spline.triggerGroups[triggerGroup].triggers[i] == null) continue;
                if (spline.triggerGroups[triggerGroup].triggers[i].Check(from, to)) AddTriggerToQueue(spline.triggerGroups[triggerGroup].triggers[i]);
            }
        }

        NodeConnection GetJunction(int pointIndex)
        {
            Node node = spline.GetNode(pointIndex);
            if (node == null) return null;
            return new NodeConnection(node, pointIndex);
        }

        protected void InvokeTriggers()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            for (int i = 0; i < m_addTriggerIndex; i++)
            {
                if (m_triggerInvokeQueue[i] != null)
                {
                    m_triggerInvokeQueue[i].Invoke(this);
                }
            }
            m_addTriggerIndex = 0;
        }

        protected void RefreshTargets()
        {
            switch (m_physicsMode)
            {
                case PhysicsMode.Transform:
                    m_targetTransform = GetTransform();
                    break;
                case PhysicsMode.Rigidbody:
                    m_targetRigidbody = GetRigidbody();
                    break;
                case PhysicsMode.Rigidbody2D:
                    m_targetRigidbody2D = GetRigidbody2D();
                    break;
            }
        }

        private void AddTriggerToQueue(SplineTrigger trigger)
        {
            if (m_addTriggerIndex >= m_triggerInvokeQueue.Length)
            {
                SplineTrigger[] newQueue = new SplineTrigger[m_triggerInvokeQueue.Length + spline.triggerGroups[triggerGroup].triggers.Length];
                m_triggerInvokeQueue.CopyTo(newQueue, 0);
                m_triggerInvokeQueue = newQueue;
            }
            m_triggerInvokeQueue[m_addTriggerIndex] = trigger;
            m_addTriggerIndex++;
        }
    }
}

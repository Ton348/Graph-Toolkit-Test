using UnityEngine;
using UnityEngine.Events;

namespace Dreamteck.Splines
{
    public delegate void SplineReachHandler();
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Follower")]
    public class SplineFollower : SplineTracer
    {
        public enum FollowMode { Uniform, Time }
        public enum Wrap { Default, Loop, PingPong }
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public FollowMode followMode = FollowMode.Uniform;

        [HideInInspector]
        public bool autoStartPosition = false;

        [SerializeField]
        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("follow")]
        private bool m_follow = true;

        [SerializeField]
        [HideInInspector]
        [Range(0f, 1f)]
        private double m_startPosition;

        /// <summary>
        /// If the follow mode is set to Uniform and there is an added offset in the motion panel, this will presserve the uniformity of the follow speed
        /// </summary>
        [HideInInspector]
        public bool preserveUniformSpeedWithOffset = false;

        /// <summary>
        /// Used when follow mode is set to Uniform. Defines the speed of the follower
        /// </summary>
        public float followSpeed
        {
            get { return m_followSpeed; }
            set
            {
                if (m_followSpeed != value)
                {
                    m_followSpeed = value;
                    Spline.Direction lastDirection = m_direction;
                    if (Mathf.Approximately(m_followSpeed, 0f)) return;
                    if (m_followSpeed < 0f)
                    {
                        direction = Spline.Direction.Backward;
                    }
                    if(m_followSpeed > 0f)
                    {
                        direction = Spline.Direction.Forward;
                    }
                }
            }
        }

        public override Spline.Direction direction {
            get {
                return base.direction;
            }
            set {
                base.direction = value;
                if(m_direction == Spline.Direction.Forward)
                {
                    if(m_followSpeed < 0f)
                    {
                        m_followSpeed = -m_followSpeed;
                    }
                } else
                {
                    if (m_followSpeed > 0f)
                    {
                        m_followSpeed = -m_followSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// Used when follow mode is set to Time. Defines how much time it takes for the follower to travel through the path
        /// </summary>
        public float followDuration
        {
            get { return m_followDuration; }
            set
            {
                if (m_followDuration != value)
                {
                    if (value < 0f) value = 0f;
                    m_followDuration = value;
                }
            }
        }

        public bool follow
        {
            get { return m_follow; }
            set
            {
                if(m_follow != value)
                {
                    if (autoStartPosition)
                    {
                        Project(GetTransform().position, ref m_evalResult);
                        SetPercent(m_evalResult.percent);
                    }
                    m_follow = value;
                }
            }
        }

        public event System.Action<double> onEndReached;
        public event System.Action<double> onBeginningReached;

        public FollowerSpeedModifier speedModifier
        {
            get
            {
                return m_speedModifier;
            }
        }

        [SerializeField]
        [HideInInspector]
        private float m_followSpeed = 1f;
        [SerializeField]
        [HideInInspector]
        private float m_followDuration = 1f;

        [SerializeField]
        [HideInInspector]
        private FollowerSpeedModifier m_speedModifier = new FollowerSpeedModifier();

        [SerializeField]
        [HideInInspector]
        private FloatEvent m_unityOnEndReached = null;
        [SerializeField]
        [HideInInspector]
        private FloatEvent m_unityOnBeginningReached = null;

        private double m_lastClippedPercent = -1.0;

        protected override void Start()
        {
            base.Start();
            if (m_follow && autoStartPosition)
            {
                SetPercent(spline.Project(GetTransform().position).percent);
            }
        }

        protected override void LateRun()
        {
            base.LateRun();
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (m_follow)
            {
                Follow();
            }
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            Evaluate(m_result.percent, ref m_result);
            if (sampleCount > 0)
            {
                if (m_follow && !autoStartPosition) ApplyMotion();
            }
        }

        private void Follow()
        {
            switch (followMode)
            {
                case FollowMode.Uniform:
                    double percent = result.percent;
                    if (!m_speedModifier.useClippedPercent)
                    {
                        UnclipPercent(ref percent);
                    }
                    float speed = m_speedModifier.GetSpeed(Mathf.Abs(m_followSpeed), percent);
                    Move(Time.deltaTime * speed); break;
                case FollowMode.Time:
                    if (m_followDuration == 0.0) Move(0.0);
                    else Move((double)Time.deltaTime / m_followDuration);
                    break;
            }
        }

        public void Restart(double startPosition = 0.0)
        {
            SetPercent(startPosition);
        }

        public override void SetPercent(double percent, bool checkTriggers = false, bool handleJunctions = false)
        {
            base.SetPercent(percent, checkTriggers, handleJunctions);
            m_lastClippedPercent = percent;

            if (!handleJunctions) return;

            InvokeNodes();
        }

        public override void SetDistance(float distance, bool checkTriggers = false, bool handleJunctions = false)
        {
            base.SetDistance(distance, checkTriggers, handleJunctions);
            m_lastClippedPercent = ClipPercent(m_result.percent);
            if (samplesAreLooped && clipFrom == clipTo && distance > 0f && m_lastClippedPercent == 0.0) m_lastClippedPercent = 1.0;

            if (!handleJunctions) return;

            InvokeNodes();
        }

        public void Move(double percent)
        {
            if (percent == 0.0) return;
            if (sampleCount <= 1)
            {
                if (sampleCount == 1)
                {
                    GetSampleRaw(0, ref m_result);
                    ApplyMotion();
                }
                return;
            }
            Evaluate(m_result.percent, ref m_result);
            double startPercent = m_result.percent;
            if (wrapMode == Wrap.Default && m_lastClippedPercent >= 1.0 && startPercent == 0.0) startPercent = 1.0;
            double p = startPercent + (m_direction == Spline.Direction.Forward ? percent : -percent);
            bool callOnEndReached = false, callOnBeginningReached = false;
            m_lastClippedPercent = p;
            if (m_direction == Spline.Direction.Forward && p >= 1.0)
            {
                if (startPercent < 1.0)
                {
                    callOnEndReached = true;
                }
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 1.0;
                        break;
                    case Wrap.Loop:
                        CheckTriggers(startPercent, 1.0);
                        CheckNodes(startPercent, 1.0);
                        while (p > 1.0) p -= 1.0;
                        startPercent = 0.0;
                        break;
                    case Wrap.PingPong:
                        p = Dmath.Clamp01(1.0 - (p - 1.0));
                        startPercent = 1.0;
                        direction = Spline.Direction.Backward;
                        break;
                }
            }
            else if (m_direction == Spline.Direction.Backward && p <= 0.0)
            {
                if (startPercent > 0.0)
                {
                    callOnBeginningReached = true;
                }
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 0.0;
                        break;
                    case Wrap.Loop:
                        CheckTriggers(startPercent, 0.0);
                        CheckNodes(startPercent, 0.0);
                        while (p < 0.0) p += 1.0;
                        startPercent = 1.0;
                        break;
                    case Wrap.PingPong:
                        p = Dmath.Clamp01(-p);
                        startPercent = 0.0;
                        direction = Spline.Direction.Forward;
                        break;
                }
            }
            CheckTriggers(startPercent, p);
            CheckNodes(startPercent, p);
            Evaluate(p, ref m_result);
            ApplyMotion();
            if (callOnEndReached)
            {
                if (onEndReached != null)
                {
                    onEndReached(startPercent);
                }
                if (m_unityOnEndReached != null)
                {
                    m_unityOnEndReached.Invoke((float)startPercent);
                }
            }
            else if (callOnBeginningReached)
            {
                if (onBeginningReached != null)
                {
                    onBeginningReached(startPercent);
                }
                if (m_unityOnBeginningReached != null)
                {
                    m_unityOnBeginningReached.Invoke((float)startPercent);
                }
            }
            InvokeTriggers();
            InvokeNodes();
        }

        public void Move(float distance)
        {
            bool endReached = false, beginningReached = false;
            float moved = 0f;
            double startPercent = m_result.percent;

            double travelPercent = DoTravel(m_result.percent, distance, out moved);
            if (startPercent != travelPercent)
            {
                CheckTriggers(startPercent, travelPercent);
                CheckNodes(startPercent, travelPercent);
            }

            if (direction == Spline.Direction.Forward)
            {
                if (travelPercent >= 1.0)
                {
                    if (startPercent < 1.0)
                    {
                        endReached = true;
                    }
                    switch (wrapMode)
                    {
                        case Wrap.Loop:
                            travelPercent = DoTravel(0.0, Mathf.Abs(distance - moved), out moved);
                            CheckTriggers(0.0, travelPercent);
                            CheckNodes(0.0, travelPercent);
                            break;
                        case Wrap.PingPong:
                            direction = Spline.Direction.Backward;
                            travelPercent = DoTravel(1.0, distance - moved, out moved);
                            CheckTriggers(1.0, travelPercent);
                            CheckNodes(1.0, travelPercent);
                            break;
                    }
                }
            } else
            {
                if (travelPercent <= 0.0)
                {
                    if (startPercent > 0.0)
                    {
                        beginningReached = true;
                    }
                    switch (wrapMode)
                    {
                        case Wrap.Loop:
                            travelPercent = DoTravel(1.0, distance - moved, out moved);
                            CheckTriggers(1.0, travelPercent);
                            CheckNodes(1.0, travelPercent);
                            break;
                        case Wrap.PingPong:
                            direction = Spline.Direction.Forward;
                            travelPercent = DoTravel(0.0, Mathf.Abs(distance - moved), out moved);
                            CheckTriggers(0.0, travelPercent);
                            CheckNodes(0.0, travelPercent);
                            break;
                    }
                }
            }

            Evaluate(travelPercent, ref m_result);
            ApplyMotion();
            if (endReached)
            {
                if (onEndReached != null)
                {
                    onEndReached(startPercent);
                }
                if (m_unityOnEndReached != null)
                {
                    m_unityOnEndReached.Invoke((float)startPercent);
                }
            }
            else if (beginningReached)
            {
                if (onBeginningReached != null)
                {
                    onBeginningReached(startPercent);
                }
                if (m_unityOnBeginningReached != null)
                {
                    m_unityOnBeginningReached.Invoke((float)startPercent);
                }
            }
            InvokeTriggers();
            InvokeNodes();
        }

        protected virtual double DoTravel(double start, float distance, out float moved)
        {
            moved = 0f;
            double result = 0.0;
            if (preserveUniformSpeedWithOffset && m_motion.hasOffset)
            {
                result = TravelWithOffset(start, distance, m_direction, m_motion.offset, out moved);
            } else
            {
                result = Travel(start, distance, m_direction, out moved);
            }
            return result;
        }

        [System.Serializable]
        public class FloatEvent : UnityEvent<float> { }
    }
}

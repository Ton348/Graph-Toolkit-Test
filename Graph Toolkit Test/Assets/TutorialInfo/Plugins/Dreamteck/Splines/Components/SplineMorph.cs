namespace Dreamteck.Splines
{
    using UnityEngine;
    [AddComponentMenu("Dreamteck/Splines/Morph")]
    public class SplineMorph : MonoBehaviour
    {
        [HideInInspector]
        public SplineComputer.Space space = SplineComputer.Space.Local;
        [HideInInspector]
        public bool cycle = false;
        public enum CycleMode {Default, Loop, PingPong}
        public enum UpdateMode {Update, FixedUpdate, LateUpdate}
        [HideInInspector]
        public CycleMode cycleMode = CycleMode.Default;
        [HideInInspector]
        public UpdateMode cycleUpdateMode = UpdateMode.Update;
        [HideInInspector]
        public float cycleDuration = 1f;
        public SplineComputer spline
        {
            get { return m_spline; }
            set
            {
                if (Application.isPlaying)
                {
                    if(m_channels.Length > 0 && value.pointCount != m_channels[0].points.Length)
                    {
                        value.SetPoints(m_channels[0].points, space);
                    }
                }
                m_spline = value;
            }
        }

        [SerializeField]
        [HideInInspector]
        private SplineComputer m_spline;
        private SplinePoint[] m_points = new SplinePoint[0];
        private float m_cycleValue = 0f;
        private short m_cycleDirection = 1;


        [System.Serializable]
        public class Channel
        {
            public enum Interpolation { Linear, Spherical }
            [SerializeField]
            internal SplinePoint[] points = new SplinePoint[0];
            [SerializeField]
            internal float percent = 1f;
            public string name = "";
            public AnimationCurve curve;
            public Interpolation interpolation = Interpolation.Linear;
        }
        [HideInInspector]
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("morphStates")]
        private Channel[] m_channels = new Channel[0];

        private void Reset()
        {
            spline = GetComponent<SplineComputer>();
        }

        private void Update()
        {
            if (cycleUpdateMode == UpdateMode.Update) RunUpdate();
        }

        private void FixedUpdate()
        {
            if (cycleUpdateMode == UpdateMode.FixedUpdate) RunUpdate();
        }

        private void LateUpdate()
        {
            if (cycleUpdateMode == UpdateMode.LateUpdate) RunUpdate();
        }

        void RunUpdate()
        {
            if (!cycle) return;
            if (cycleMode != CycleMode.PingPong) m_cycleDirection = 1;
            m_cycleValue += Time.deltaTime / cycleDuration * m_cycleDirection;
            switch (cycleMode)
            {
                case CycleMode.Default:
                    if (m_cycleValue > 1f) m_cycleValue = 1f;
                    break;
                case CycleMode.Loop:
                    if (m_cycleValue > 1f) m_cycleValue -= Mathf.Floor(m_cycleValue);
                    break;
                case CycleMode.PingPong:
                    if (m_cycleValue > 1f)
                    {
                        m_cycleValue = 1f - (m_cycleValue - Mathf.Floor(m_cycleValue));
                        m_cycleDirection = -1;
                    } else if (m_cycleValue < 0f)
                    {
                        m_cycleValue = -m_cycleValue - Mathf.Floor(-m_cycleValue);
                        m_cycleDirection = 1;
                    }
                    break;
            }
            SetWeight(m_cycleValue, cycleMode == CycleMode.Loop);
        }

        public void SetCycle(float value)
        {
            m_cycleValue = Mathf.Clamp01(value);
        }

        public void SetWeight(int index, float weight)
        {
            m_channels[index].percent = Mathf.Clamp01(weight);
            UpdateMorph();
        }

        public void SetWeight(string name, float weight)
        {
            int index = GetChannelIndex(name);
            m_channels[index].percent = Mathf.Clamp01(weight);
            UpdateMorph();
        }

        public void SetWeight(float percent, bool loop = false)
        {
            float channelValue = percent * (loop ? m_channels.Length : m_channels.Length - 1);
            for (int i = 0; i < m_channels.Length; i++)
            {
                float delta = Mathf.Abs(i - channelValue);
                if (delta > 1f)
                {
                    SetWeight(i, 0f);
                }
                else
                {
                    if (channelValue <= i)
                    {
                        SetWeight(i, 1f - (i - channelValue));
                    }
                    else
                    {
                        SetWeight(i, 1f - (channelValue - i));
                    }
                }
            }
            if (loop && channelValue >= m_channels.Length - 1)
            {
                SetWeight(0, channelValue - (m_channels.Length - 1));
            }
        }

        public void CaptureSnapshot(string name)
        {
            CaptureSnapshot(GetChannelIndex(name));
        }

        public void CaptureSnapshot(int index)
        {
            if (m_spline == null) return;
            if ((m_channels.Length > 0 && m_spline.pointCount != m_channels[0].points.Length && index != 0))
            {
                Debug.LogError("Point count must be the same as " + m_spline.pointCount);
                return;
            }
            m_channels[index].points = m_spline.GetPoints(space);
            UpdateMorph();
        }

        public void Clear()
        {
            m_channels = new Channel[0];
        }

        public SplinePoint[] GetSnapshot(int index)
        {
            return m_channels[index].points;
        }
        public void SetSnapshot(int index, SplinePoint[] points)
        {
            m_channels[index].points = points;
        }


        public SplinePoint[] GetSnapshot(string name)
        {
            int index = GetChannelIndex(name);
            return m_channels[index].points;
        }

        public float GetWeight(int index)
        {
            return m_channels[index].percent;
        }

        public float GetWeight(string name)
        {
            int index = GetChannelIndex(name);
            return m_channels[index].percent;
        }

        public void AddChannel(string name)
        {
            if (m_spline == null) return;
            if (m_channels.Length > 0 && m_spline.pointCount != m_channels[0].points.Length)
            {
                Debug.LogError("Point count must be the same as " + m_channels[0].points.Length);
                return;
            }
            Channel newMorph = new Channel();
            newMorph.points = m_spline.GetPoints(space);
            newMorph.name = name;
            newMorph.curve = new AnimationCurve();
            newMorph.curve.AddKey(new Keyframe(0, 0, 0, 1));
            newMorph.curve.AddKey(new Keyframe(1, 1, 1, 0));
            ArrayUtility.Add(ref m_channels, newMorph);
            UpdateMorph();
        }

        public void RemoveChannel(string name)
        {
            int index = GetChannelIndex(name);
            RemoveChannel(index);
        }

        public void RemoveChannel(int index)
        {
            if (index < 0 || index >= m_channels.Length) return;
            Channel[] newStates = new Channel[m_channels.Length - 1];
            for (int i = 0; i < m_channels.Length; i++)
            {
                if (i == index) continue;
                else if (i < index) newStates[i] = m_channels[i];
                else if (i >= index) newStates[i - 1] = m_channels[i];
            }
            m_channels = newStates;
            UpdateMorph();
        }

        private int GetChannelIndex(string name)
        {
            for (int i = 0; i < m_channels.Length; i++)
            {
                if (m_channels[i].name == name)
                {
                    return i;
                }
            }
            Debug.Log("Channel not found " + name);
            return 0;
        }

        public int GetChannelCount()
        {
            if (m_channels == null) return 0;
            return m_channels.Length;
        }

        public Channel GetChannel(int index)
        {
            return m_channels[index];
        }

        public Channel GetChannel(string name)
        {
            return m_channels[GetChannelIndex(name)];
        }

        public void UpdateMorph()
        {
            if (m_spline == null) return;
            if (m_channels.Length == 0) return;
            if(m_points.Length != m_channels[0].points.Length)
            {
                m_points = new SplinePoint[m_channels[0].points.Length];
            }

            for (int i = 0; i < m_channels.Length; i++)
            {
                for (int j = 0; j < m_points.Length; j++)
                {
                    if(i == 0)
                    {
                        m_points[j] = m_channels[0].points[j];
                        continue;
                    }

                    float percent = m_channels[i].curve.Evaluate(m_channels[i].percent);
                    if (m_channels[i].interpolation == Channel.Interpolation.Linear)
                    {
                        m_points[j].position += (m_channels[i].points[j].position - m_channels[0].points[j].position) * percent;
                        m_points[j].tangent += (m_channels[i].points[j].tangent - m_channels[0].points[j].tangent) * percent;
                        m_points[j].tangent2 += (m_channels[i].points[j].tangent2 - m_channels[0].points[j].tangent2) * percent;
                        m_points[j].normal += (m_channels[i].points[j].normal - m_channels[0].points[j].normal) * percent;
                    } else
                    {
                        m_points[j].position = Vector3.Slerp(m_points[j].position, m_points[j].position + (m_channels[i].points[j].position - m_channels[0].points[j].position), percent);
                        m_points[j].tangent = Vector3.Slerp(m_points[j].tangent, m_points[j].tangent + (m_channels[i].points[j].tangent - m_channels[0].points[j].tangent), percent);
                        m_points[j].tangent2 = Vector3.Slerp(m_points[j].tangent2, m_points[j].tangent2 + (m_channels[i].points[j].tangent2 - m_channels[0].points[j].tangent2), percent);
                        m_points[j].normal = Vector3.Slerp(m_points[j].normal, m_points[j].normal + (m_channels[i].points[j].normal - m_channels[0].points[j].normal), percent);
                    }

                    m_points[j].color += (m_channels[i].points[j].color - m_channels[0].points[j].color) * percent;
                    m_points[j].size += (m_channels[i].points[j].size - m_channels[0].points[j].size) * percent;

                    if(m_points[j].type == SplinePoint.Type.SmoothMirrored) m_points[j].type = m_channels[i].points[j].type;
                    else if(m_points[j].type == SplinePoint.Type.SmoothFree)
                    {
                        if (m_channels[i].points[j].type == SplinePoint.Type.Broken) m_points[j].type = SplinePoint.Type.Broken;
                    }
                }
            }

            for (int i = 0; i < m_points.Length; i++)
            {
                m_points[i].normal.Normalize();
            }
            m_spline.SetPoints(m_points, space);
        }
    }
}

namespace Dreamteck.Splines
{
    using UnityEngine;

    //Use the CreateAssetMenu attribute to add the object to the Create Asset context menu
    //After that, go to Assets/Create/Dreamteck/Splines/... and create the scriptable object
    [CreateAssetMenu(menuName = "Dreamteck/Splines/Object Controller Rules/Sine Rule")]
    public class ObjectControllerSineRule : ObjectControllerCustomRuleBase
    {
        [SerializeField] private bool m_useSplinePercent = false;
        [SerializeField] private float m_frequency = 1f;
        [SerializeField] private float m_amplitude = 1f;
        [SerializeField] private float m_angle = 0f;
        [SerializeField] private float m_minScale = 1f;
        [SerializeField] private float m_maxScale = 1f;
        [SerializeField] [Range(0f, 1f)] private float m_offset = 0f;

        public bool useSplinePercent
        {
            get { return m_useSplinePercent; }
            set { m_useSplinePercent = value; }
        }

        public float frequency
        {
            get { return m_frequency; }
            set { m_frequency = value; }
        }

        public float amplitude
        {
            get { return m_amplitude; }
            set { m_amplitude = value; }
        }

        public float angle
        {
            get { return m_angle; }
            set { m_angle = value; }
        }

        public float minScale
        {
            get { return m_minScale; }
            set { m_minScale = value; }
        }

        public float maxScale
        {
            get { return m_maxScale; }
            set { m_maxScale = value; }
        }

        public float offset
        {
            get { return m_offset; }
            set { 
                m_offset = value;
                if(m_offset > 1)
                {
                    m_offset -= Mathf.FloorToInt(m_offset);
                }
                if(m_offset < 0)
                {
                    m_offset += Mathf.FloorToInt(-m_offset);
                }
            }
        }

        //Override GetOffset, GetRotation and GetScale to implement custom behaviors
        //Use the information from currentSample, currentObjectIndex, totalObjects and currentObjectPercent

        public override Vector3 GetOffset()
        {
            float sin = GetSine();
            return Quaternion.AngleAxis(m_angle, Vector3.forward) * Vector3.up * sin * m_amplitude;
        }

        public override Vector3 GetScale()
        {
            return Vector3.Lerp(Vector3.one * m_minScale, Vector3.one * m_maxScale, GetSine());
        }

        private float GetSine()
        {
            float objectPercent = m_useSplinePercent ? (float)m_currentSample.percent : currentObjectPercent;
            return Mathf.Sin((Mathf.PI * m_offset) + objectPercent * Mathf.PI * m_frequency);
        }
    }
}

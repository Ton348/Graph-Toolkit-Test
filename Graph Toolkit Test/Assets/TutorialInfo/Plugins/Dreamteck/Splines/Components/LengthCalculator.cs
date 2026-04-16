using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEngine.Events;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Users/Length Calculator")]
    public class LengthCalculator : SplineUser
    {
        [System.Serializable]
        public class LengthEvent
        {
            public bool enabled = true;
            public float targetLength = 0f;
            public UnityEvent onChange = new UnityEvent();
            public enum Type { Growing, Shrinking, Both}
            public Type type = Type.Both;

            public LengthEvent()
            {

            }

            public LengthEvent(Type t)
            {
                type = t;
            }

            public void Check(float fromLength, float toLength)
            {
                if (!enabled) return;
                bool condition = false;
                switch (type)
                {
                    case Type.Growing: condition = toLength >= targetLength && fromLength < targetLength; break;
                    case Type.Shrinking: condition = toLength <= targetLength && fromLength > targetLength; break;
                    case Type.Both: condition = toLength >= targetLength && fromLength < targetLength || toLength <= targetLength && fromLength > targetLength; break;
                }
                if (condition) onChange.Invoke();
            }
        }
        [HideInInspector]
        public LengthEvent[] lengthEvents = new LengthEvent[0];
        [HideInInspector]
        public float idealLength = 1f;
        private float m_length = 0f;
        private float m_lastLength = 0f;
        public float length
        {
            get {
                return m_length;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_length = CalculateLength();
            m_lastLength = m_length;
            for (int i = 0; i < lengthEvents.Length; i++)
            {
                if (lengthEvents[i].targetLength == m_length) lengthEvents[i].onChange.Invoke();
            }
        }

        protected override void Build()
        {
            base.Build();
            m_length = CalculateLength();
            if (m_lastLength != m_length)
            {
                for (int i = 0; i < lengthEvents.Length; i++)
                {
                    lengthEvents[i].Check(m_lastLength, m_length);
                }
                m_lastLength = m_length;
            }
        }

        public void AddEvent(LengthEvent lengthEvent)
        {
            LengthEvent[] newEvents = new LengthEvent[lengthEvents.Length + 1];
            lengthEvents.CopyTo(newEvents, 0);
            newEvents[newEvents.Length - 1] = lengthEvent;
            lengthEvents = newEvents;
        }
    }
}

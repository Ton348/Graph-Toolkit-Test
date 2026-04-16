namespace Dreamteck.Utilities
{
    using UnityEngine;

    public class Randomizer
    {
        private int m_seed;
        private System.Random m_random;

        public System.Random random => m_random;

        public Randomizer(int seed)
        {
            m_seed = seed;
            m_random = new System.Random(m_seed);
        }

        public float Random01()
        {
            return (float)m_random.NextDouble();
        }

        public float Random(float min, float max)
        {
            return (float)Dmath.Lerp(min, max, m_random.NextDouble());
        }

        public int Random(int min, int max)
        {
            return (int)Dmath.Lerp(min, max, m_random.NextDouble());
        }

        public Vector2 RandomVector2(float min, float max)
        {
            return new Vector2(Random(min, max), Random(min, max));
        }

        public Vector3 RandomVector3(float min, float max)
        {
            return new Vector3(Random(min, max), Random(min, max), Random(min, max));
        }

        public Vector3 OnUnitSphere()
        {
            return Quaternion.Euler(Random(0f, 360f), Random(0f, 360f), Random(0f, 360f)) * Vector3.forward;
        }

        public Vector3 OnUnitCircle()
        {
            return Quaternion.AngleAxis(Random(0f, 360f), Vector3.forward) * Vector3.up;
        }

        public Vector3 InsideUnitSphere()
        {
            return OnUnitSphere() * Random01();
        }

        public Vector3 InsideUnitCircle()
        {
            return OnUnitCircle() * Random01();
        }

        public void Reset()
        {
            m_random = new System.Random(m_seed);
        }

        public void Reset(int seed)
        {
            m_random = new System.Random(seed);
        }
    }
}
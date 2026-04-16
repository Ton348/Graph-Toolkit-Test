using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [System.Serializable]
    public class ObjectSequence<T>
    {
        public T startObject;
        public T endObject;
        public T[] objects;
        public enum Iteration { Ordered, Random }
        public Iteration iteration = Iteration.Ordered;
        public int randomSeed
        {
            get { return m_randomSeed; }
            set
            {
                if (value != m_randomSeed)
                {
                    m_randomSeed = value;
                    m_randomizer = new System.Random(m_randomSeed);
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private int m_randomSeed = 1;
        [SerializeField]
        [HideInInspector]
        private int m_index = 0;
        [SerializeField]
        [HideInInspector]
        System.Random m_randomizer;
        
        public ObjectSequence(){
            m_randomizer = new System.Random(m_randomSeed);
        }

        public T GetFirst()
        {
            if (startObject != null) return startObject;
            else return Next();
        }

        public T GetLast()
        {
            if (endObject != null) return endObject;
            else return Next();
        }

        public T Next()
        {
            if (iteration == Iteration.Ordered)
            {
                if (m_index >= objects.Length) m_index = 0;
                return objects[m_index++];
            } else
            {
                int randomIndex = m_randomizer.Next(objects.Length-1);
                return objects[randomIndex];
            }
        }
    }
}
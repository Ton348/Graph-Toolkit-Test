using System;
using UnityEngine;
using Random = System.Random;

namespace Dreamteck.Splines
{
	[Serializable]
	public class ObjectSequence<T>
	{
		public enum Iteration
		{
			Ordered,
			Random
		}

		public T startObject;
		public T endObject;
		public T[] objects;
		public Iteration iteration = Iteration.Ordered;

		[SerializeField]
		[HideInInspector]
		private int m_randomSeed = 1;

		[SerializeField]
		[HideInInspector]
		private int m_index;

		private Random m_randomizer;

		public ObjectSequence()
		{
			m_randomizer = new Random(m_randomSeed);
		}

		public int randomSeed
		{
			get => m_randomSeed;
			set
			{
				if (value != m_randomSeed)
				{
					m_randomSeed = value;
					m_randomizer = new Random(m_randomSeed);
				}
			}
		}

		public T GetFirst()
		{
			if (startObject != null)
			{
				return startObject;
			}

			return Next();
		}

		public T GetLast()
		{
			if (endObject != null)
			{
				return endObject;
			}

			return Next();
		}

		public T Next()
		{
			if (iteration == Iteration.Ordered)
			{
				if (m_index >= objects.Length)
				{
					m_index = 0;
				}

				return objects[m_index++];
			}

			int randomIndex = m_randomizer.Next(objects.Length - 1);
			return objects[randomIndex];
		}
	}
}
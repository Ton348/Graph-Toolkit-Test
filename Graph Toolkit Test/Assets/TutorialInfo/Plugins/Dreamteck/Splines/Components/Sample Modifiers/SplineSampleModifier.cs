using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
	[Serializable]
	public class SplineSampleModifier
	{
		[Range(0f, 1f)]
		public float blend = 1f;

		public bool useClippedPercent;

		public virtual bool hasKeys => false;

		public virtual List<Key> GetKeys()
		{
			return new List<Key>();
		}

		public virtual void SetKeys(List<Key> input)
		{
		}

		public virtual void Apply(ref SplineSample result)
		{
		}

		public virtual void Apply(ref SplineSample source, ref SplineSample destination)
		{
			destination = source;
			Apply(ref destination);
		}

		[Serializable]
		public class Key
		{
			[SerializeField]
			private double m_featherStart, m_featherEnd, m_centerStart = 0.25, m_centerEnd = 0.75;

			public AnimationCurve interpolation;
			public float blend = 1f;

			internal Key(double f, double t)
			{
				start = f;
				end = t;
				interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}

			public double start
			{
				get => m_featherStart;
				set
				{
					if (value != m_featherStart)
					{
						m_featherStart = Dmath.Clamp01(value);
					}
				}
			}

			public double end
			{
				get => m_featherEnd;
				set
				{
					if (value != m_featherEnd)
					{
						m_featherEnd = Dmath.Clamp01(value);
					}
				}
			}

			public double centerStart
			{
				get => m_centerStart;
				set
				{
					if (value != m_centerStart)
					{
						m_centerStart = Dmath.Clamp01(value);
						if (m_centerStart > m_centerEnd)
						{
							m_centerStart = m_centerEnd;
						}
					}
				}
			}

			public double centerEnd
			{
				get => m_centerEnd;
				set
				{
					if (value != m_centerEnd)
					{
						m_centerEnd = Dmath.Clamp01(value);
						if (m_centerEnd < m_centerStart)
						{
							m_centerEnd = m_centerStart;
						}
					}
				}
			}


			public double globalCenterStart
			{
				get => LocalToGlobalPercent(centerStart);
				set => centerStart = Dmath.Clamp01(GlobalToLocalPercent(value));
			}

			public double globalCenterEnd
			{
				get => LocalToGlobalPercent(centerEnd);
				set => centerEnd = Dmath.Clamp01(GlobalToLocalPercent(value));
			}

			public double position
			{
				get
				{
					double center = Dmath.Lerp(m_centerStart, m_centerEnd, 0.5);
					if (start > end)
					{
						double fromToEndDistance = 1.0 - m_featherStart;
						double centerDistance = center * (fromToEndDistance + m_featherEnd);
						double pos = m_featherStart + centerDistance;
						if (pos > 1.0)
						{
							pos -= 1.0;
						}

						return pos;
					}

					return Dmath.Lerp(m_featherStart, m_featherEnd, center);
				}
				set
				{
					double delta = value - position;
					start += delta;
					end += delta;
				}
			}

			private double GlobalToLocalPercent(double t)
			{
				if (m_featherStart > m_featherEnd)
				{
					if (t > m_featherStart)
					{
						return Dmath.InverseLerp(m_featherStart, m_featherStart + (1.0 - m_featherStart) + m_featherEnd,
							t);
					}

					if (t < m_featherEnd)
					{
						return Dmath.InverseLerp(-(1.0 - m_featherStart), m_featherEnd, t);
					}

					return 0f;
				}

				return Dmath.InverseLerp(m_featherStart, m_featherEnd, t);
			}

			private double LocalToGlobalPercent(double t)
			{
				if (m_featherStart > m_featherEnd)
				{
					t = Dmath.Lerp(m_featherStart, m_featherStart + (1.0 - m_featherStart) + m_featherEnd, t);
					if (t > 1.0)
					{
						t -= 1.0;
					}

					return t;
				}

				return Dmath.Lerp(m_featherStart, m_featherEnd, t);
			}

			public float Evaluate(double t)
			{
				t = (float)GlobalToLocalPercent(t);
				if (t < m_centerStart)
				{
					return interpolation.Evaluate((float)(t / m_centerStart)) * blend;
				}

				if (t > m_centerEnd)
				{
					return interpolation.Evaluate(1f - (float)Dmath.InverseLerp(m_centerEnd, 1.0, t)) * blend;
				}

				return interpolation.Evaluate(1f) * blend;
			}

			public virtual Key Duplicate()
			{
				var newKey = new Key(start, end);
				newKey.m_centerStart = m_centerStart;
				newKey.m_centerEnd = m_centerEnd;
				newKey.blend = blend;
				newKey.interpolation = DuplicateUtility.DuplicateCurve(interpolation);
				return newKey;
			}
		}
	}
}
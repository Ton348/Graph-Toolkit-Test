using UnityEngine;
using System.Collections.Generic;

namespace Prototype.Business.NPC.Danger
{
	public sealed class DangerManager : MonoBehaviour
	{
		public static DangerManager Instance { get; private set; }
		[SerializeField] private float defaultRadius = 30f;
		[SerializeField] private int defaultThreatScore = 1;
		[SerializeField] private float defaultLifetimeSeconds = 8f;
		[SerializeField] private DangerSourceType defaultSourceType = DangerSourceType.Gunshot;
		private readonly List<DangerSource> m_sources = new List<DangerSource>(32);
		private int m_nextSourceId = 1;

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		private void Update()
		{
			float now = UnityEngine.Time.time;
			for (int i = m_sources.Count - 1; i >= 0; i--)
			{
				DangerSource src = m_sources[i];
				if (src == null)
				{
					m_sources.RemoveAt(i);
					continue;
				}

				if (src.FollowTarget != null)
				{
					src.Position = src.FollowTarget.position;
				}

				if (src.IsContagion)
				{
					DangerSource root = FindActiveSourceById(src.RootSourceId, now);
					if (root == null)
					{
						m_sources.RemoveAt(i);
						continue;
					}

					src.ThreatScore = root.ThreatScore;
					src.SourceType = root.SourceType;
					src.ExpireAt = root.ExpireAt;
					src.Lifetime = Mathf.Max(0f, root.ExpireAt - now);
				}

				if (src.FollowTarget == null && src.IsContagion)
				{
					m_sources.RemoveAt(i);
					continue;
				}

				if (!src.IsActive(now))
				{
					m_sources.RemoveAt(i);
				}
			}
		}

		public void RaiseDangerEvent(Vector3 dangerPosition, float dangerRadius, int threatScore, float dangerTimer, DangerSourceType dangerSource)
		{
			DangerSource source = new DangerSource
			{
				SourceId = m_nextSourceId++,
				RootSourceId = 0,
				Position = dangerPosition,
				Radius = Mathf.Max(0f, dangerRadius),
				ThreatScore = threatScore,
				Lifetime = Mathf.Max(0f, dangerTimer),
				SourceType = dangerSource,
				ExpireAt = UnityEngine.Time.time + Mathf.Max(0f, dangerTimer)
			};
			m_sources.Add(source);
			source.RootSourceId = source.SourceId;
		}

		public void RaiseDefaultDangerAt(Vector3 dangerPosition)
		{
			float now = UnityEngine.Time.time;
			for (int i = 0; i < m_sources.Count; i++)
			{
				DangerSource src = m_sources[i];
				if (src == null || !src.IsActive(now))
				{
					continue;
				}

				float radius = Mathf.Max(0f, src.Radius);
				float sqr = (dangerPosition - src.Position).sqrMagnitude;
				if (sqr > radius * radius)
				{
					continue;
				}

				src.ThreatScore += Mathf.Max(1, defaultThreatScore);
				src.Lifetime = Mathf.Max(src.Lifetime, defaultLifetimeSeconds);
				src.ExpireAt = now + Mathf.Max(0f, defaultLifetimeSeconds);
				return;
			}

			RaiseDangerEvent(
				dangerPosition,
				defaultRadius,
				Mathf.Max(1, defaultThreatScore),
				defaultLifetimeSeconds,
				defaultSourceType);
		}

		public void SetNpcContagionSource(int ownerId, Transform followTarget, float radius, int rootSourceId, bool enabled)
		{
			float now = UnityEngine.Time.time;
			DangerSource root = FindActiveSourceById(rootSourceId, now);

			for (int i = 0; i < m_sources.Count; i++)
			{
				DangerSource src = m_sources[i];
				if (src == null || !src.IsContagion || src.OwnerId != ownerId)
				{
					continue;
				}

				if (!enabled || followTarget == null)
				{
					m_sources.RemoveAt(i);
					return;
				}
				if (root == null)
				{
					m_sources.RemoveAt(i);
					return;
				}

				src.FollowTarget = followTarget;
				src.Position = followTarget.position;
				src.Radius = Mathf.Max(0f, radius);
				src.RootSourceId = root.SourceId;
				src.Lifetime = Mathf.Max(0f, root.ExpireAt - now);
				src.ExpireAt = root.ExpireAt;
				src.SourceType = root.SourceType;
				src.ThreatScore = root.ThreatScore;
				return;
			}

			if (!enabled || followTarget == null || root == null)
			{
				return;
			}

			m_sources.Add(new DangerSource
			{
				SourceId = m_nextSourceId++,
				RootSourceId = root.SourceId,
				Position = followTarget.position,
				Radius = Mathf.Max(0f, radius),
				ThreatScore = root.ThreatScore,
				Lifetime = Mathf.Max(0f, root.ExpireAt - now),
				SourceType = root.SourceType,
				ExpireAt = root.ExpireAt,
				FollowTarget = followTarget,
				OwnerId = ownerId,
				IsContagion = true
			});
		}

		private DangerSource FindActiveSourceById(int sourceId, float now)
		{
			if (sourceId <= 0)
			{
				return null;
			}

			for (int i = 0; i < m_sources.Count; i++)
			{
				DangerSource src = m_sources[i];
				if (src == null || src.SourceId != sourceId)
				{
					continue;
				}

				if (!src.IsActive(now))
				{
					return null;
				}

				return src;
			}

			return null;
		}

		public bool TryGetNearestActiveSource(Vector3 position, out DangerSource source)
		{
			float now = UnityEngine.Time.time;
			source = null;
			float bestSqr = float.MaxValue;
			for (int i = 0; i < m_sources.Count; i++)
			{
				DangerSource s = m_sources[i];
				if (s == null || !s.IsActive(now))
				{
					continue;
				}

				float radius = Mathf.Max(0f, s.Radius);
				float sqr = (position - s.Position).sqrMagnitude;
				if (sqr > radius * radius)
				{
					continue;
				}

				if (sqr < bestSqr)
				{
					bestSqr = sqr;
					source = s;
				}
			}

			return source != null;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			float now = Application.isPlaying ? UnityEngine.Time.time : 0f;
			for (int i = 0; i < m_sources.Count; i++)
			{
				DangerSource s = m_sources[i];
				if (s == null)
				{
					continue;
				}
				if (Application.isPlaying && !s.IsActive(now))
				{
					continue;
				}
				Gizmos.DrawWireSphere(s.Position, Mathf.Max(0f, s.Radius));
			}
		}
	}
}

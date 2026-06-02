using UnityEngine;

namespace Prototype.Business.NPC.Danger
{
	public sealed class DangerSource
	{
		public int SourceId;
		public int RootSourceId;
		public Vector3 OriginPosition;
		public Vector3 Position;
		public float Radius;
		public int ThreatScore;
		public float Lifetime;
		public DangerSourceType SourceType;
		public float ExpireAt;
		public Transform FollowTarget;
		public int OwnerId;
		public bool IsContagion;

		public bool IsActive(float now)
		{
			return now < ExpireAt;
		}
	}
}

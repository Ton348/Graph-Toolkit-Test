using UnityEngine;

namespace Prototype.Business.NPC.Danger
{
	public struct DangerEvent
	{
		public Vector3 dangerPosition;
		public float dangerRadius;
		public int threatScore;
		public float dangerTimer;
		public DangerSourceType dangerSource;
	}
}

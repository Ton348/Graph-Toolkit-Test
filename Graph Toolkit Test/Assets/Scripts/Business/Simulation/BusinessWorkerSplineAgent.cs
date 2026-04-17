using Dreamteck.Splines;
using UnityEngine;

namespace Prototype.Business.Simulation
{
	public class BusinessWorkerSplineAgent : MonoBehaviour
	{
		public SplineFollower follower;
		public float followSpeed = 1.5f;
		public int carryBatch = 10;

		public bool IsWaiting { get; private set; }
		public float CurrentCarryAmount { get; private set; }
		public bool CarryingGoods => CurrentCarryAmount > 0f;

		private void Awake()
		{
			if (follower == null)
			{
				follower = GetComponent<SplineFollower>();
			}
		}

		public void BindRoute(SplineComputer route, Transform spawnPoint, int triggerGroupIndex)
		{
			if (follower == null || route == null)
			{
				return;
			}

			follower.spline = route;
			follower.followMode = SplineFollower.FollowMode.Uniform;
			follower.wrapMode = SplineFollower.Wrap.Loop;
			follower.followSpeed = followSpeed;
			follower.useTriggers = true;
			follower.triggerGroup = Mathf.Max(0, triggerGroupIndex);
			follower.RebuildImmediate();
			follower.Restart();

			if (spawnPoint != null)
			{
				transform.position = spawnPoint.position;
				transform.rotation = spawnPoint.rotation;
			}

			follower.SetPercent(0.0);
			follower.follow = true;
		}

		public void StopMovement()
		{
			IsWaiting = true;
			if (follower != null)
			{
				follower.follow = false;
			}
		}

		public void ResumeMovement()
		{
			IsWaiting = false;
			if (follower != null)
			{
				follower.follow = true;
			}
		}

		public void SetCarry(float amount)
		{
			CurrentCarryAmount = Mathf.Max(0f, amount);
		}

		public void ClearCarry()
		{
			CurrentCarryAmount = 0f;
		}
	}
}
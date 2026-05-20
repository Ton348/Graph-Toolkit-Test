using UnityEngine;

namespace Prototype.Business.World
{
	public class BusinessCashierPoint : MonoBehaviour
	{
		public BusinessWorldRuntime worldRuntime;
		public string playerTag = "Player";

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag(playerTag))
			{
				return;
			}

		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.CompareTag(playerTag))
			{
				return;
			}

		}
	}
}

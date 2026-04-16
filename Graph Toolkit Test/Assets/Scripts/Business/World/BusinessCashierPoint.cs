using UnityEngine;

public class BusinessCashierPoint : MonoBehaviour
{
	public BusinessWorldRuntime worldRuntime;
	public float cashierMultiplier = 2f;
	public string playerTag = "Player";

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag(playerTag))
		{
			return;
		}

		ApplyMultiplier(true);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag(playerTag))
		{
			return;
		}

		ApplyMultiplier(false);
	}

	private void ApplyMultiplier(bool active)
	{
		if (worldRuntime == null)
		{
			worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
		}

		if (worldRuntime == null)
		{
			return;
		}

		BusinessSimulationService simulation = worldRuntime.GetSimulationService();
		if (simulation == null)
		{
			return;
		}

		simulation.SetCashierMultiplier(worldRuntime.lotId, active ? cashierMultiplier : 1f);
		BusinessDebugLog.Log(
			$"[BusinessWorld] Cashier point {(active ? "entered" : "exited")} lotId='{worldRuntime.lotId}' multiplier={(active ? cashierMultiplier : 1f)}");
	}
}
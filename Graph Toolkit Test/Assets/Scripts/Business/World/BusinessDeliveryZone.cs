using UnityEngine;

public class BusinessDeliveryZone : MonoBehaviour
{
    public BusinessWorldRuntime worldRuntime;
    public int deliveryAmount = 100;
    public string deliveryItemId = "goods";
    public bool requireItem = true;
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (!Input.GetKeyDown(interactKey))
        {
            return;
        }

        var carrier = other.GetComponentInParent<PlayerCarryItem>() ?? other.GetComponent<PlayerCarryItem>();
        if (requireItem && (carrier == null || !carrier.HasItem(deliveryItemId)))
        {
            return;
        }

        TryDeliver(carrier);
    }

    private async void TryDeliver(PlayerCarryItem carrier)
    {
        if (worldRuntime == null)
        {
            worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
        }

        if (worldRuntime == null)
        {
            BusinessDebugLog.Warn("[BusinessWorld] Delivery zone missing BusinessWorldRuntime.");
            return;
        }

        var facade = worldRuntime.GetActionFacade();
        if (facade == null)
        {
            BusinessDebugLog.Warn("[BusinessWorld] BusinessActionFacade missing.");
            return;
        }

        BusinessDebugLog.Log($"[BusinessWorld] Deliver stock lotId='{worldRuntime.lotId}' amount={deliveryAmount}");
        var result = await facade.AddBusinessStock(worldRuntime.lotId, deliveryAmount);
        if (result != null && result.Success && carrier != null && requireItem)
        {
            carrier.TryConsume(deliveryItemId);
            BusinessDebugLog.Log($"[BusinessWorld] Delivery success lotId='{worldRuntime.lotId}'");
        }
        else if (result != null && !result.Success)
        {
            BusinessDebugLog.Warn($"[BusinessWorld] Delivery failed lotId='{worldRuntime.lotId}' error={result.ErrorCode}");
        }
    }
}

using UnityEngine;

public class PlayerCarryItem : MonoBehaviour
{
    public string currentItemId;
    public int amount = 1;

    public bool HasItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && currentItemId == itemId && amount > 0;
    }

    public bool TryConsume(string itemId, int consumeAmount = 1)
    {
        if (!HasItem(itemId) || consumeAmount <= 0)
        {
            return false;
        }

        if (amount < consumeAmount)
        {
            return false;
        }

        amount -= consumeAmount;
        if (amount <= 0)
        {
            currentItemId = null;
            amount = 0;
        }
        return true;
    }

    public void SetItem(string itemId, int newAmount = 1)
    {
        currentItemId = itemId;
        amount = Mathf.Max(0, newAmount);
    }
}

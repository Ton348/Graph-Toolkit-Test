using TMPro;
using UnityEngine;

public class MoneyUi : MonoBehaviour
{
    public GameBootstrap bootstrap;
    public TMP_Text moneyText;

    private void Update()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (moneyText == null || bootstrap == null || bootstrap.PlayerStateSync == null)
        {
            return;
        }

        moneyText.text = $"Money: {bootstrap.PlayerStateSync.Money}";
    }
}

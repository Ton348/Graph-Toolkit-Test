[System.Obsolete("Legacy client runtime. Only LocalGameServer uses this service.")]
public class PlayerService
{
    private readonly PlayerProfileState player;

    public PlayerService(PlayerProfileState player)
    {
        this.player = player;
    }

    public bool HasEnoughMoney(int amount)
    {
        if (player == null)
        {
            return false;
        }

        return player.Money >= amount;
    }

    public void SpendMoney(int amount)
    {
        if (player == null || amount <= 0)
        {
            return;
        }

        if (!HasEnoughMoney(amount))
        {
            return;
        }

        player.Money -= amount;
    }

    public void AddMoney(int amount)
    {
        if (player == null || amount <= 0)
        {
            return;
        }

        player.Money += amount;
    }
}

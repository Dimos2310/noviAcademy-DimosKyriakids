namespace WorldRank;

// Thrown when an operation targets a wallet the player doesn't have.
public class WalletNotFoundException : WalletException
{
    public int PlayerId { get; }
    public Currency Currency { get; }

    public WalletNotFoundException(int playerId, Currency currency)
        : base($"Player {playerId} does not have a {currency} wallet.")
    {
        PlayerId = playerId;
        Currency = currency;
    }
}

namespace WorldRank;

// Thrown when a player is given a second wallet of a currency they already hold.
public class DuplicateWalletException : WalletException
{
    public int PlayerId { get; }
    public Currency Currency { get; }

    public DuplicateWalletException(int playerId, Currency currency)
        : base($"Player {playerId} already has a {currency} wallet.")
    {
        PlayerId = playerId;
        Currency = currency;
    }
}

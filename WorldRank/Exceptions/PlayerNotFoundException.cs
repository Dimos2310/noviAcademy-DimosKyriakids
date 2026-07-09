namespace WorldRank;

// Thrown when an operation references a player that doesn't exist.
public class PlayerNotFoundException : Exception
{
    public int PlayerId { get; }

    public PlayerNotFoundException(int playerId)
        : base($"Player {playerId} was not found.")
    {
        PlayerId = playerId;
    }
}

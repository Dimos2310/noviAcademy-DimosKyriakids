namespace WorldRank.Domain.Exceptions;

public class WorldRankException : Exception
{
    public WorldRankException(string message) : base(message) { }
    public WorldRankException(string message, Exception inner) : base(message, inner) { }
}

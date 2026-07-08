namespace WorldRank;

public class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players = new();

    public void AddPlayer(Player player) => _players.Add(player);

    public Player? FindPlayer(int playerId) =>
        _players.FirstOrDefault(p => p.Id == playerId);

    public void DeletePlayer(int playerId)
    {
        var player = FindPlayer(playerId);
        if (player is not null)
            _players.Remove(player);
    }

    public IEnumerable<IGrouping<int, Player>> GroupPlayersByScore() =>
        _players.GroupBy(p => p.Score);

    public IEnumerable<Player> GetAll() => _players;
}

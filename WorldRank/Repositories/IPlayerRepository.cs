namespace WorldRank;

public interface IPlayerRepository
{
    // Adding requires a concrete Player — IPlayer has no way to construct one
    void AddPlayer(Player player);

    // Reads traffic in IPlayer, not the concrete Player, so callers only see the contract
    IPlayer? FindPlayer(int playerId);
    void DeletePlayer(int playerId);
    IEnumerable<IGrouping<int, IPlayer>> GroupPlayersByScore();

    // Needed by the console menu to list all players and search by name
    IEnumerable<IPlayer> GetAll();
}

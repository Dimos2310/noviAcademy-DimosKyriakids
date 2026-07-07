namespace WorldRank;

public interface IPlayerRepository
{
    void AddPlayer(Player player);
    Player? FindPlayer(int playerId);
    void DeletePlayer(int playerId);
    IEnumerable<IGrouping<int, Player>> GroupPlayersByScore();

    // Needed by the console menu to list all players and search by name
    IEnumerable<Player> GetAll();
}

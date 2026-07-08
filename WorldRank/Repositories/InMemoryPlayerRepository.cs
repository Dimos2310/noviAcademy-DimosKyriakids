namespace WorldRank;

public class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players = new();
    private readonly IWalletRepository _walletRepository;

    public InMemoryPlayerRepository(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public void AddPlayer(Player player) => _players.Add(player);

    public IPlayer? FindPlayer(int playerId) =>
        _players.FirstOrDefault(p => p.Id == playerId);

    public void DeletePlayer(int playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player is null)
            return;

        _players.Remove(player);

        // Cascade: a deleted player shouldn't leave orphaned wallets behind
        _walletRepository.RemoveByPlayer(playerId);
    }

    public IEnumerable<IGrouping<int, IPlayer>> GroupPlayersByScore() =>
        _players.GroupBy(p => p.Score, p => (IPlayer)p);

    // AsReadOnly() wraps the live list — a caller cannot cast it back
    // and mutate the repository's internal state
    public IEnumerable<IPlayer> GetAll() => _players.AsReadOnly();
}

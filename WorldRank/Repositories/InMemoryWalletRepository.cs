namespace WorldRank;

// Links wallets to players by id; a player can hold at most one wallet per currency
public class InMemoryWalletRepository : IWalletRepository
{
    private readonly Dictionary<int, List<Wallet>> _walletsByPlayer = new();

    public void Add(Wallet wallet, int playerId)
    {
        if (!_walletsByPlayer.TryGetValue(playerId, out var wallets))
        {
            wallets = new List<Wallet>();
            _walletsByPlayer[playerId] = wallets;
        }

        if (wallets.Any(w => w.Currency == wallet.Currency))
            throw new InvalidOperationException(
                $"Player {playerId} already has a {wallet.Currency} wallet.");

        wallets.Add(wallet);
    }

    public IEnumerable<Wallet> GetByPlayer(int playerId) =>
        _walletsByPlayer.TryGetValue(playerId, out var wallets)
            ? wallets
            : Enumerable.Empty<Wallet>();
}

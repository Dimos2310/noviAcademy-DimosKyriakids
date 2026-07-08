namespace WorldRank;

// Links wallets to players by id; a player can hold at most one wallet per currency
public class InMemoryWalletRepository : IWalletRepository
{
    private readonly Dictionary<int, List<Wallet>> _walletsByPlayer = new();

    public void Add(Wallet wallet, int playerId)
    {
        ArgumentNullException.ThrowIfNull(wallet);

        if (!_walletsByPlayer.TryGetValue(playerId, out var wallets))
        {
            wallets = new List<Wallet>();
            // This line matters: without storing the new list back in the
            // dictionary, wallets.Add below would mutate a list nobody keeps,
            // and GetByPlayer would return nothing. (See DEBUGGING.md.)
            _walletsByPlayer[playerId] = wallets;
        }

        if (wallets.Any(w => w.Currency == wallet.Currency))
            throw new DuplicateWalletException(playerId, wallet.Currency);

        wallets.Add(wallet);
    }

    public IEnumerable<IWallet> GetByPlayer(int playerId)
    {
        // AsReadOnly() wraps the live list — a caller cannot cast it back
        // and mutate the repository's internal state
        if (_walletsByPlayer.TryGetValue(playerId, out var wallets))
            return wallets.AsReadOnly();

        return Array.Empty<IWallet>();
    }

    public void RemoveByPlayer(int playerId) => _walletsByPlayer.Remove(playerId);
}

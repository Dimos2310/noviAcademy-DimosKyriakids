using NLog;

namespace WorldRank;

// Links wallets to players by id; a player can hold at most one wallet per currency.
// Logs every successful mutation at the service layer with structured properties.
public class InMemoryWalletRepository : IWalletRepository
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
        _logger.Info("Wallet added for player {PlayerId} in {Currency}", playerId, wallet.Currency);
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

    public void Deposit(int playerId, Currency currency, decimal amount)
    {
        var wallet = GetWallet(playerId, currency);
        wallet.Deposit(amount);
        _logger.Info("Deposit of {Amount} into player {PlayerId} {Currency} wallet succeeded (balance {Balance})",
            amount, playerId, currency, wallet.Balance);
    }

    public void Withdraw(int playerId, Currency currency, decimal amount)
    {
        var wallet = GetWallet(playerId, currency);
        wallet.Withdraw(amount);
        _logger.Info("Withdrawal of {Amount} from player {PlayerId} {Currency} wallet succeeded (balance {Balance})",
            amount, playerId, currency, wallet.Balance);
    }

    public void Block(int playerId, Currency currency)
    {
        GetWallet(playerId, currency).Block();
        _logger.Info("Player {PlayerId} {Currency} wallet blocked", playerId, currency);
    }

    public void Unblock(int playerId, Currency currency)
    {
        GetWallet(playerId, currency).Unblock();
        _logger.Info("Player {PlayerId} {Currency} wallet unblocked", playerId, currency);
    }

    // Resolves the one wallet a player holds in a currency, or throws.
    private Wallet GetWallet(int playerId, Currency currency)
    {
        if (_walletsByPlayer.TryGetValue(playerId, out var wallets))
        {
            var wallet = wallets.FirstOrDefault(w => w.Currency == currency);
            if (wallet is not null)
                return wallet;
        }

        throw new WalletNotFoundException(playerId, currency);
    }
}

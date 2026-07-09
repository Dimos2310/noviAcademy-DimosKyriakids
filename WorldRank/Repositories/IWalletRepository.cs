namespace WorldRank;

public interface IWalletRepository
{
    // Adding requires a concrete Wallet — IWallet has no way to construct one
    void Add(Wallet wallet, int playerId);

    // Reads traffic in IWallet, not the concrete Wallet
    IEnumerable<IWallet> GetByPlayer(int playerId);

    // Cascades a player deletion so wallets don't outlive their owner
    void RemoveByPlayer(int playerId);

    // Mutations resolve the wallet by (playerId, currency) and log at this
    // (service) layer, so every caller gets the audit trail for free.
    void Deposit(int playerId, Currency currency, decimal amount);
    void Withdraw(int playerId, Currency currency, decimal amount);
    void Block(int playerId, Currency currency);
    void Unblock(int playerId, Currency currency);
}

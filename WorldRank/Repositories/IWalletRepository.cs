namespace WorldRank;

public interface IWalletRepository
{
    // Adding requires a concrete Wallet — IWallet has no way to construct one
    void Add(Wallet wallet, int playerId);

    // Reads traffic in IWallet, not the concrete Wallet
    IEnumerable<IWallet> GetByPlayer(int playerId);

    // Cascades a player deletion so wallets don't outlive their owner
    void RemoveByPlayer(int playerId);
}

namespace WorldRank;

public interface IWallet
{
    decimal Balance { get; }
    Currency Currency { get; }
    bool IsBlocked { get; }

    void Deposit(decimal amount);
    void Withdraw(decimal amount);
    void Block();
    void Unblock();
}

namespace WorldRank;

// Wallet: Balance changes only through Deposit/Withdraw, never goes negative,
// and no transaction is allowed while the wallet is blocked
public class Wallet : IWallet
{
    public decimal Balance { get; private set; }
    public Currency Currency { get; }
    public bool IsBlocked { get; private set; }

    public Wallet(Currency currency)
    {
        Currency = currency;
        Balance = 0m;
        IsBlocked = false;
    }

    public void Deposit(decimal amount)
    {
        if (IsBlocked)
            throw new InvalidOperationException("Wallet is blocked.");

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive.");

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (IsBlocked)
            throw new InvalidOperationException("Wallet is blocked.");

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Withdraw amount must be positive.");

        if (amount > Balance)
            throw new InvalidOperationException("Insufficient balance.");

        Balance -= amount;
    }

    public void Block() => IsBlocked = true;
    public void Unblock() => IsBlocked = false;

    public override string ToString() =>
        $"{Currency}: {Balance:0.00}{(IsBlocked ? " [BLOCKED]" : "")}";
}

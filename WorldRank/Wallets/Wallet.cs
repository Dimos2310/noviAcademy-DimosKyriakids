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
        // Guard clauses: reject early, keep the happy path flat and last.
        // amount <= 0 is a CALLER bug -> ArgumentOutOfRangeException.
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive.");

        // Blocked is a BUSINESS rule violation -> custom WalletException.
        if (IsBlocked)
            throw new WalletBlockedException(Currency);

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Withdraw amount must be positive.");

        if (IsBlocked)
            throw new WalletBlockedException(Currency);

        // Negative balance is a business rule violation -> custom WalletException
        // that carries the balance and the requested amount as typed data.
        if (amount > Balance)
            throw new InsufficientFundsException(Currency, Balance, amount);

        Balance -= amount;
    }

    public void Block() => IsBlocked = true;
    public void Unblock() => IsBlocked = false;

    public override string ToString() =>
        $"{Currency}: {Balance:0.00}{(IsBlocked ? " [BLOCKED]" : "")}";
}

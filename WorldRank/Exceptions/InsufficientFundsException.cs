namespace WorldRank;

// Thrown when a withdrawal would push a wallet's balance below zero.
// Carries typed data so a caller can react programmatically, not just read a string.
public class InsufficientFundsException : WalletException
{
    public Currency Currency { get; }
    public decimal Balance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientFundsException(Currency currency, decimal balance, decimal requestedAmount)
        : base($"Cannot withdraw {requestedAmount:0.00} from a {currency} wallet holding {balance:0.00}.")
    {
        Currency = currency;
        Balance = balance;
        RequestedAmount = requestedAmount;
    }
}

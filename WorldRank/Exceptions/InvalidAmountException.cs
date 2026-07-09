namespace WorldRank;

// Thrown when a deposit or withdrawal amount is not strictly positive.
// Carries the offending amount as typed data.
public class InvalidAmountException : WalletException
{
    public decimal Amount { get; }

    public InvalidAmountException(decimal amount)
        : base($"The amount must be greater than zero (provided: {amount}).")
    {
        Amount = amount;
    }
}

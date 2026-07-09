namespace WorldRank;

// Thrown when a deposit or withdrawal is attempted on a blocked wallet.
public class WalletBlockedException : WalletException
{
    public Currency Currency { get; }

    public WalletBlockedException(Currency currency)
        : base($"The {currency} wallet is blocked and cannot process transactions.")
    {
        Currency = currency;
    }
}

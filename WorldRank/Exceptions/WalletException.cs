namespace WorldRank;

// Abstract base for every wallet-related business rule violation.
// Catching WalletException catches the whole family in one clause.
public abstract class WalletException : Exception
{
    protected WalletException(string message) : base(message) { }

    protected WalletException(string message, Exception innerException)
        : base(message, innerException) { }
}

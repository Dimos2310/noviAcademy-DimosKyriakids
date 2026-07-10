namespace WorldRank.Domain.Exceptions
{
	public abstract class WalletException : WorldRankException
	{
		protected WalletException(string message) : base(message)
		{
		}

		protected WalletException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

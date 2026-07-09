namespace WorldRank.Console.Exceptions
{
	public abstract class WalletException : Exception
	{
		protected WalletException(string message) : base(message)
		{
		}

		protected WalletException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

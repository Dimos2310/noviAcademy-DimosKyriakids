namespace WorldRank.Domain.Exceptions
{
	public class InsufficientFundsException : WalletException
	{
		public decimal AttemptedBalance { get; }

		public InsufficientFundsException(decimal attemptedBalance)
			: base($"Insufficient funds: wallet balance cannot be negative (attempted balance: {attemptedBalance}).")
		{
			AttemptedBalance = attemptedBalance;
		}
	}
}

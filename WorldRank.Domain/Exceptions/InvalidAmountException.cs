namespace WorldRank.Domain.Exceptions
{
	public class InvalidAmountException : WalletException
	{
		public decimal Amount { get; }

		public InvalidAmountException(decimal amount)
			: base($"The amount must be greater than zero (provided: {amount}).")
		{
			Amount = amount;
		}
	}
}

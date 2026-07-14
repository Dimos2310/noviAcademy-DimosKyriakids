namespace WorldRank.Domain.Exceptions
{
	public class WalletNotFoundByIdException : WalletException
	{
		public int WalletId { get; }

		public WalletNotFoundByIdException(int walletId)
			: base($"Wallet {walletId} was not found.")
		{
			WalletId = walletId;
		}
	}
}

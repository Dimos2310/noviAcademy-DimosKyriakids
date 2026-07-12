using WorldRank.Domain.Enums;

namespace WorldRank.Domain.Exceptions
{
	public class DuplicateWalletException : WalletException
	{
		public int PlayerId { get; }
		public Currency Currency { get; }

		public DuplicateWalletException(int playerId, Currency currency)
			: base($"Player {playerId} already has a wallet in {currency}.")
		{
			PlayerId = playerId;
			Currency = currency;
		}
	}
}

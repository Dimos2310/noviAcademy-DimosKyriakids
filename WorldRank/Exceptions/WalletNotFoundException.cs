using WorldRank.Console.Enums;

namespace WorldRank.Console.Exceptions
{
	public class WalletNotFoundException : WalletException
	{
		public int PlayerId { get; }
		public Currency Currency { get; }

		public WalletNotFoundException(int playerId, Currency currency)
			: base($"Player {playerId} does not have a wallet in {currency}.")
		{
			PlayerId = playerId;
			Currency = currency;
		}
	}
}

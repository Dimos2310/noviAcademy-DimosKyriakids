using WorldRank.Domain.Enums;

namespace WorldRank.Domain.Exceptions
{
	public class WalletBlockedException : WalletException
	{
		public Currency Currency { get; }

		public WalletBlockedException(Currency currency)
			: base($"The {currency} wallet is blocked. Unblock it before performing this operation.")
		{
			Currency = currency;
		}
	}
}

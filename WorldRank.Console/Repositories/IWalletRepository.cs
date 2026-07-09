using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Console
{
	public interface IWalletRepository
	{
		void Add(Wallet wallet);

		List<Wallet> GetAllWalletsByPlayerId(int playerId);

		void UpdateBalance(int playerId, Currency currency, decimal newBalance);

		void Deposit(int playerId, Currency currency, decimal amount);

		void Withdraw(int playerId, Currency currency, decimal amount);

		void Block(int playerId, Currency currency);

		void Unblock(int playerId, Currency currency);
	}
}

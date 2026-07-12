using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletRepository
{
	void Add(Wallet wallet);

	Wallet[] GetAll();
	List<Wallet> GetAllWalletsByPlayerId(int playerId);

	Wallet GetWallet(int playerId, Currency currency);

	void UpdateBalance(int playerId, Currency currency, decimal newBalance);

	void Deposit(int playerId, Currency currency, decimal amount);

	void Withdraw(int playerId, Currency currency, decimal amount);

	void Block(int playerId, Currency currency);

	void Unblock(int playerId, Currency currency);
}

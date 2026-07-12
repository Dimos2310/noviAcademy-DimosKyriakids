using WorldRank.Application.Strategies;
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

	// Applies a funds strategy to a wallet and persists the result in one step,
	// so every mutating operation self-saves (no separate Save() to remember).
	void ApplyStrategy(int playerId, Currency currency, IFundsStrategy strategy, decimal amount);
}

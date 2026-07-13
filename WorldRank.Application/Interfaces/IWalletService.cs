using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletService
{
	void AddWalletToPlayer(int playerId, Currency currency, decimal initialBalance);

	IReadOnlyList<Wallet> GetWalletsOfPlayer(int playerId);

	void Deposit(int playerId, Currency currency, decimal amount);

	void Withdraw(int playerId, Currency currency, decimal amount);

	void Block(int playerId, Currency currency);

	void Unblock(int playerId, Currency currency);

	void UpdateBalance(int playerId, Currency currency, decimal newBalance);

	void ApplyFunds(int playerId, Currency currency, FundsOperation operation, decimal amount);
}

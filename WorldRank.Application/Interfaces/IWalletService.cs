using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletService
{
	void ExecuteOperation(int playerId, Currency currency, decimal amount, FundsOperation operation);
}

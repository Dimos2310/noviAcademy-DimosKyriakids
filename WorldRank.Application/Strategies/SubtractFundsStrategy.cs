using WorldRank.Domain.Entities;

namespace WorldRank.Application.Strategies;

public class SubtractFundsStrategy : IFundsStrategy
{
	public FundsOperation Operation => FundsOperation.Subtract;
	public void Execute(Wallet wallet, decimal amount) => wallet.Withdraw(amount);
}

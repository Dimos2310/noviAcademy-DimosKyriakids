using WorldRank.Domain.Entities;

namespace WorldRank.Application.Strategies;

/// Strategy pattern: a family of algorithms behind one interface.
/// Each strategy knows which operation it implements, so the caller can select it
/// among all registered strategies without a factory.

public interface IFundsStrategy
{
	FundsOperation Operation { get; }
	void Execute(Wallet wallet, decimal amount);
}

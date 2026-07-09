using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Services;

public class WalletService : IWalletService
{
	private readonly IWalletRepository _walletRepository;
	private readonly IEnumerable<IFundsStrategy> _strategies;

	public WalletService(IWalletRepository walletRepository, IEnumerable<IFundsStrategy> strategies)
	{
		_walletRepository = walletRepository;
		_strategies = strategies;
	}

	public void ExecuteOperation(int playerId, Currency currency, decimal amount, FundsOperation operation)
	{
		var wallet = _walletRepository.GetWallet(playerId, currency);
		var strategy = _strategies.FirstOrDefault(s => s.Operation == operation);

		if (strategy == null)
		{
			throw new InvalidOperationException($"No strategy found for operation {operation}");
		}

		strategy.Execute(wallet, amount);
	}
}

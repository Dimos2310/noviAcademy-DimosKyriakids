using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class ApplyFundsCommandHandler : IRequestHandler<ApplyFundsCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;
	private readonly IReadOnlyDictionary<FundsOperation, IFundsStrategy> _fundsStrategies;

	public ApplyFundsCommandHandler(IWalletRepository walletRepository, IEnumerable<IFundsStrategy> strategies)
	{
		_walletRepository = walletRepository;
		_fundsStrategies = strategies.ToDictionary(strategy => strategy.Operation);
	}

	public Task<Wallet> Handle(ApplyFundsCommand request, CancellationToken cancellationToken)
	{
		var strategy = _fundsStrategies[request.Operation];
		return _walletRepository.ApplyStrategyAsync(request.WalletId, strategy, request.Amount, cancellationToken);
	}
}

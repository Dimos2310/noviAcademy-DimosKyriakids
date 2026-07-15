using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class ApplyFundsCommandHandler : IRequestHandler<ApplyFundsCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;
	private readonly IReadOnlyDictionary<FundsOperation, IFundsStrategy> _fundsStrategies;

	public ApplyFundsCommandHandler(IWalletWriteRepository walletWriteRepository, IEnumerable<IFundsStrategy> strategies)
	{
		_walletWriteRepository = walletWriteRepository;
		_fundsStrategies = strategies.ToDictionary(strategy => strategy.Operation);
	}

	public Task<Wallet> Handle(ApplyFundsCommand request, CancellationToken cancellationToken)
	{
		var strategy = _fundsStrategies[request.Operation];
		return _walletWriteRepository.ApplyStrategyAsync(request.WalletId, strategy, request.Amount, cancellationToken);
	}
}

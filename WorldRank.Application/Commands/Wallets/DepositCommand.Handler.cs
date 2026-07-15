using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class DepositCommandHandler : IRequestHandler<DepositCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public DepositCommandHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(DepositCommand request, CancellationToken cancellationToken)
		=> _walletRepository.DepositAsync(request.WalletId, request.Amount, cancellationToken);
}

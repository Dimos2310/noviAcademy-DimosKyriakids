using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class UpdateWalletBalanceCommandHandler : IRequestHandler<UpdateWalletBalanceCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public UpdateWalletBalanceCommandHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(UpdateWalletBalanceCommand request, CancellationToken cancellationToken)
		=> _walletRepository.UpdateBalanceAsync(request.WalletId, request.NewBalance, cancellationToken);
}

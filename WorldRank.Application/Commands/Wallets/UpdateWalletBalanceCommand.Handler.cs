using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class UpdateWalletBalanceCommandHandler : IRequestHandler<UpdateWalletBalanceCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;

	public UpdateWalletBalanceCommandHandler(IWalletWriteRepository walletWriteRepository)
	{
		_walletWriteRepository = walletWriteRepository;
	}

	public Task<Wallet> Handle(UpdateWalletBalanceCommand request, CancellationToken cancellationToken)
		=> _walletWriteRepository.UpdateBalanceAsync(request.WalletId, request.NewBalance, cancellationToken);
}

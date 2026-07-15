using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class UnblockWalletCommandHandler : IRequestHandler<UnblockWalletCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public UnblockWalletCommandHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(UnblockWalletCommand request, CancellationToken cancellationToken)
		=> _walletRepository.UnblockAsync(request.WalletId, cancellationToken);
}

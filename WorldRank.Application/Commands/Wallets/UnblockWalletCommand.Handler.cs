using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class UnblockWalletCommandHandler : IRequestHandler<UnblockWalletCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;

	public UnblockWalletCommandHandler(IWalletWriteRepository walletWriteRepository)
	{
		_walletWriteRepository = walletWriteRepository;
	}

	public Task<Wallet> Handle(UnblockWalletCommand request, CancellationToken cancellationToken)
		=> _walletWriteRepository.UnblockAsync(request.WalletId, cancellationToken);
}

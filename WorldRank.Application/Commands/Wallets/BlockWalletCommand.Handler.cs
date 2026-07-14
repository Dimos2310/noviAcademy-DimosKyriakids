using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class BlockWalletCommandHandler : IRequestHandler<BlockWalletCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public BlockWalletCommandHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(BlockWalletCommand request, CancellationToken cancellationToken)
		=> _walletRepository.BlockAsync(request.WalletId, cancellationToken);
}

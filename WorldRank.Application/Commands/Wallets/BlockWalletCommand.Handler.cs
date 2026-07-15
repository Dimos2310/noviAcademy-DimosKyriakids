using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class BlockWalletCommandHandler : IRequestHandler<BlockWalletCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;

	public BlockWalletCommandHandler(IWalletWriteRepository walletWriteRepository)
	{
		_walletWriteRepository = walletWriteRepository;
	}

	public Task<Wallet> Handle(BlockWalletCommand request, CancellationToken cancellationToken)
		=> _walletWriteRepository.BlockAsync(request.WalletId, cancellationToken);
}

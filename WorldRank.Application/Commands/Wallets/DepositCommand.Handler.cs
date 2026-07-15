using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class DepositCommandHandler : IRequestHandler<DepositCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;

	public DepositCommandHandler(IWalletWriteRepository walletWriteRepository)
	{
		_walletWriteRepository = walletWriteRepository;
	}

	public Task<Wallet> Handle(DepositCommand request, CancellationToken cancellationToken)
		=> _walletWriteRepository.DepositAsync(request.WalletId, request.Amount, cancellationToken);
}

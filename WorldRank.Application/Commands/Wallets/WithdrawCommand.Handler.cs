using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class WithdrawCommandHandler : IRequestHandler<WithdrawCommand, Wallet>
{
	private readonly IWalletWriteRepository _walletWriteRepository;

	public WithdrawCommandHandler(IWalletWriteRepository walletWriteRepository)
	{
		_walletWriteRepository = walletWriteRepository;
	}

	public Task<Wallet> Handle(WithdrawCommand request, CancellationToken cancellationToken)
		=> _walletWriteRepository.WithdrawAsync(request.WalletId, request.Amount, cancellationToken);
}

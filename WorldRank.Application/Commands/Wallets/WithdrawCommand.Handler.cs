using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public class WithdrawCommandHandler : IRequestHandler<WithdrawCommand, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public WithdrawCommandHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(WithdrawCommand request, CancellationToken cancellationToken)
		=> _walletRepository.WithdrawAsync(request.WalletId, request.Amount, cancellationToken);
}

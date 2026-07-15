using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Application.Commands.Wallets;

public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, int>
{
	private readonly IWalletWriteRepository _walletWriteRepository;
	private readonly IPlayerReadRepository _playerReadRepository;

	public CreateWalletCommandHandler(IWalletWriteRepository walletWriteRepository, IPlayerReadRepository playerReadRepository)
	{
		_walletWriteRepository = walletWriteRepository;
		_playerReadRepository = playerReadRepository;
	}

	public async Task<int> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
	{
		if (await _playerReadRepository.FindPlayerAsync(request.PlayerId, cancellationToken) is null)
			throw new PlayerNotFoundException(request.PlayerId);

		var wallet = new Wallet(request.PlayerId, request.Currency, request.InitialBalance);
		await _walletWriteRepository.AddAsync(wallet, cancellationToken);
		return wallet.Id;
	}
}

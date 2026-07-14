using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Application.Commands.Wallets;

public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, int>
{
	private readonly IWalletRepository _walletRepository;
	private readonly IPlayerRepository _playerRepository;

	public CreateWalletCommandHandler(IWalletRepository walletRepository, IPlayerRepository playerRepository)
	{
		_walletRepository = walletRepository;
		_playerRepository = playerRepository;
	}

	public async Task<int> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
	{
		if (await _playerRepository.FindPlayerAsync(request.PlayerId, cancellationToken) is null)
			throw new PlayerNotFoundException(request.PlayerId);

		var wallet = new Wallet(request.PlayerId, request.Currency, request.InitialBalance);
		await _walletRepository.AddAsync(wallet, cancellationToken);
		return wallet.Id;
	}
}

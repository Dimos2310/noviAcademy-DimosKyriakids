using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletByIdQueryHandler : IRequestHandler<GetWalletByIdQuery, Wallet?>
{
	private readonly IWalletRepository _walletRepository;

	public GetWalletByIdQueryHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet?> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
		=> _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
}

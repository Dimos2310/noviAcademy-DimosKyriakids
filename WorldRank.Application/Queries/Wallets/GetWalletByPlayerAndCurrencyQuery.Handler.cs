using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletByPlayerAndCurrencyQueryHandler : IRequestHandler<GetWalletByPlayerAndCurrencyQuery, Wallet>
{
	private readonly IWalletRepository _walletRepository;

	public GetWalletByPlayerAndCurrencyQueryHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public Task<Wallet> Handle(GetWalletByPlayerAndCurrencyQuery request, CancellationToken cancellationToken)
		=> _walletRepository.GetWalletAsync(request.PlayerId, request.Currency, cancellationToken);
}

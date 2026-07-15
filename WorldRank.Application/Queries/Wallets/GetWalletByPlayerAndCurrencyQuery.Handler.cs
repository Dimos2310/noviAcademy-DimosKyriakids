using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletByPlayerAndCurrencyQueryHandler : IRequestHandler<GetWalletByPlayerAndCurrencyQuery, Wallet>
{
	private readonly IWalletReadRepository _walletReadRepository;

	public GetWalletByPlayerAndCurrencyQueryHandler(IWalletReadRepository walletReadRepository)
	{
		_walletReadRepository = walletReadRepository;
	}

	public Task<Wallet> Handle(GetWalletByPlayerAndCurrencyQuery request, CancellationToken cancellationToken)
		=> _walletReadRepository.GetWalletAsync(request.PlayerId, request.Currency, cancellationToken);
}

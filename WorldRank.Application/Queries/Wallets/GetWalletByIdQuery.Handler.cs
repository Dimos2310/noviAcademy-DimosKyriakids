using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletByIdQueryHandler : IRequestHandler<GetWalletByIdQuery, Wallet?>
{
	private readonly IWalletReadRepository _walletReadRepository;

	public GetWalletByIdQueryHandler(IWalletReadRepository walletReadRepository)
	{
		_walletReadRepository = walletReadRepository;
	}

	public Task<Wallet?> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
		=> _walletReadRepository.GetByIdAsync(request.WalletId, cancellationToken);
}

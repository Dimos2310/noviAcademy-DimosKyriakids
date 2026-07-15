using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletsByPlayerQueryHandler : IRequestHandler<GetWalletsByPlayerQuery, IReadOnlyList<Wallet>>
{
	private readonly IWalletReadRepository _walletReadRepository;

	public GetWalletsByPlayerQueryHandler(IWalletReadRepository walletReadRepository)
	{
		_walletReadRepository = walletReadRepository;
	}

	public async Task<IReadOnlyList<Wallet>> Handle(GetWalletsByPlayerQuery request, CancellationToken cancellationToken)
		=> await _walletReadRepository.GetAllWalletsByPlayerIdAsync(request.PlayerId, cancellationToken);
}

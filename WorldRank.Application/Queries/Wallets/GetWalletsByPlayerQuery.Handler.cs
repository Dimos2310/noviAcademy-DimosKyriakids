using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public class GetWalletsByPlayerQueryHandler : IRequestHandler<GetWalletsByPlayerQuery, IReadOnlyList<Wallet>>
{
	private readonly IWalletRepository _walletRepository;

	public GetWalletsByPlayerQueryHandler(IWalletRepository walletRepository)
	{
		_walletRepository = walletRepository;
	}

	public async Task<IReadOnlyList<Wallet>> Handle(GetWalletsByPlayerQuery request, CancellationToken cancellationToken)
		=> await _walletRepository.GetAllWalletsByPlayerIdAsync(request.PlayerId, cancellationToken);
}

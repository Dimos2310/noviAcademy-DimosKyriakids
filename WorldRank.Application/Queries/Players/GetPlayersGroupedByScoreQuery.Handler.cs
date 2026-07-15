using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetPlayersGroupedByScoreQueryHandler : IRequestHandler<GetPlayersGroupedByScoreQuery, IEnumerable<IGrouping<int, Player>>>
{
	private readonly IPlayerReadRepository _playerReadRepository;

	public GetPlayersGroupedByScoreQueryHandler(IPlayerReadRepository playerReadRepository)
	{
		_playerReadRepository = playerReadRepository;
	}

	public Task<IEnumerable<IGrouping<int, Player>>> Handle(GetPlayersGroupedByScoreQuery request, CancellationToken cancellationToken)
		=> _playerReadRepository.GroupPlayersByScoreAsync(cancellationToken);
}

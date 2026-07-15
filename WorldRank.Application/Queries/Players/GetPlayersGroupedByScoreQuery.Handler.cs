using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetPlayersGroupedByScoreQueryHandler : IRequestHandler<GetPlayersGroupedByScoreQuery, IEnumerable<IGrouping<int, Player>>>
{
	private readonly IPlayerRepository _playerRepository;

	public GetPlayersGroupedByScoreQueryHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public Task<IEnumerable<IGrouping<int, Player>>> Handle(GetPlayersGroupedByScoreQuery request, CancellationToken cancellationToken)
		=> _playerRepository.GroupPlayersByScoreAsync(cancellationToken);
}

using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetAllPlayersQueryHandler : IRequestHandler<GetAllPlayersQuery, IReadOnlyList<Player>>
{
	private readonly IPlayerRepository _playerRepository;

	public GetAllPlayersQueryHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public Task<IReadOnlyList<Player>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
		=> _playerRepository.GetAllPlayersAsync(cancellationToken);
}

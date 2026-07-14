using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetPlayerByNameQueryHandler : IRequestHandler<GetPlayerByNameQuery, Player?>
{
	private readonly IPlayerRepository _playerRepository;

	public GetPlayerByNameQueryHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public async Task<Player?> Handle(GetPlayerByNameQuery request, CancellationToken cancellationToken)
	{
		// Reuses the cached "all players" list (via the repository decorator) instead of a
		// separate cache entry per name.
		var players = await _playerRepository.GetAllPlayersAsync(cancellationToken);
		return players.FirstOrDefault(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
	}
}

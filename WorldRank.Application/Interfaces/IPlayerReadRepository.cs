using WorldRank.Domain.Entities;

namespace WorldRank.Application.Interfaces;

public interface IPlayerReadRepository
{
	Task<IReadOnlyList<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default);

	Task<Player?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default);

	Task<IEnumerable<IGrouping<int, Player>>> GroupPlayersByScoreAsync(CancellationToken cancellationToken = default);
}

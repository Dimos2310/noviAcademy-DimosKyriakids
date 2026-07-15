using WorldRank.Domain.Entities;

namespace WorldRank.Application.Interfaces;

public interface IPlayerWriteRepository
{
	Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default);

	Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default);
}

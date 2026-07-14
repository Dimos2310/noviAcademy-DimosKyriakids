using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Repositories;

public class InMemoryPlayerRepository : IPlayerRepository
{
	private readonly ILogger<InMemoryPlayerRepository> _logger;

	private readonly List<Player> _players = new();

	public InMemoryPlayerRepository(ILogger<InMemoryPlayerRepository> logger)
	{
		_logger = logger;
	}

	public Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
	{
		// Assign the next id here: the store owns id generation, not the service.
		var id = _players.Count == 0 ? 1 : _players.Max(item => item.Id) + 1;
		var stored = new Player(id, player.Name, player.Score);

		_players.Add(stored);
		_logger.LogInformation("Player {PlayerId} ({Name}) added with score {Score}", stored.Id, stored.Name, stored.Score);

		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
	{
		// Return a copy so callers cannot mutate the repository's internal list.
		return Task.FromResult<IReadOnlyList<Player>>(_players.ToList());
	}

	public Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		var player = _players.FirstOrDefault(item => item.Id == playerId);

		if (player is null)
		{
			_logger.LogWarning("Delete skipped: player {PlayerId} not found", playerId);
			return Task.CompletedTask;
		}

		_players.Remove(player);
		_logger.LogInformation("Player {PlayerId} deleted", playerId);

		return Task.CompletedTask;
	}

	public Task<Player?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_players.FirstOrDefault(item => item.Id == playerId));
	}

	public Task<IEnumerable<IGrouping<int, Player>>> GroupPlayersByScoreAsync(CancellationToken cancellationToken = default)
	{
		var groups = _players
			.GroupBy(player => player.Score)
			.OrderByDescending(group => group.Key)
			.AsEnumerable();

		return Task.FromResult(groups);
	}
}

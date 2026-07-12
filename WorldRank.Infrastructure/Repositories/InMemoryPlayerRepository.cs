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

	public void AddPlayer(Player player)
	{
		// Assign the next id here: the store owns id generation, not the service.
		var id = _players.Count == 0 ? 1 : _players.Max(item => item.Id) + 1;
		var stored = new Player(id, player.Name, player.Score);

		_players.Add(stored);
		_logger.LogInformation("Player {PlayerId} ({Name}) added with score {Score}", stored.Id, stored.Name, stored.Score);
	}

	public IEnumerable<Player> GetAllPlayers()
	{
		// Return a copy so callers cannot mutate the repository's internal list.
		return _players.ToList();
	}

	public void DeletePlayer(int playerId)
	{
		var player = _players.FirstOrDefault(item => item.Id == playerId);

		if (player is null)
		{
			_logger.LogWarning("Delete skipped: player {PlayerId} not found", playerId);
			return;
		}

		_players.Remove(player);
		_logger.LogInformation("Player {PlayerId} deleted", playerId);
	}

	public Player? FindPlayer(int playerId)
	{
		return _players.FirstOrDefault(item => item.Id == playerId);
	}

	public IEnumerable<IGrouping<int, Player>> GroupPlayersByScore()
	{
		return _players
			.GroupBy(player => player.Score)
			.OrderByDescending(group => group.Key);
	}
}

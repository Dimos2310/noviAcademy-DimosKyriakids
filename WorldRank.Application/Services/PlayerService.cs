using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Services;

// Use-case logic only. No console / presentation concerns: inputs come in as
// parameters, results are returned, failures surface as domain exceptions.
public class PlayerService : IPlayerService
{
	private const string AllPlayersCacheKey = "AllPlayersKey";
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private readonly IPlayerRepository _playerRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<PlayerService> _logger;

	public PlayerService(IPlayerRepository playerRepository, IMemoryCache cache, ILogger<PlayerService> logger)
	{
		_playerRepository = playerRepository;
		_cache = cache;
		_logger = logger;
	}

	public Player AddPlayer(string name, int score)
	{
		// The id is assigned by the store (database identity or the in-memory
		// repository), so the service never generates one itself.
		var player = new Player(name);
		player.AddScore(score);
		_playerRepository.AddPlayer(player);

		// Write-through: the cached list is now stale, drop it so the next read reloads it.
		_cache.Remove(AllPlayersCacheKey);

		return player;
	}

	public IEnumerable<Player> GetAllPlayers()
	{
		if (_cache.TryGetValue(AllPlayersCacheKey, out IReadOnlyList<Player>? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT all players");
			return cached;
		}

		_logger.LogInformation("Cache MISS all players — loading from database");
		var players = _playerRepository.GetAllPlayers().ToList();

		_cache.Set(AllPlayersCacheKey, (IReadOnlyList<Player>)players, CacheTtl);

		return players;
	}

	public IEnumerable<IGrouping<int, Player>> GroupPlayersByScore()
	{
		return _playerRepository.GroupPlayersByScore();
	}

	public Player? FindPlayerByName(string name)
	{
		return _playerRepository.GetAllPlayers()
			.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	public Player? FindPlayerById(int playerId)
	{
		return _playerRepository.FindPlayer(playerId);
	}

	public void DeletePlayer(int playerId)
	{
		_playerRepository.DeletePlayer(playerId);

		// Write-through: the cached list is now stale, drop it so the next read reloads it.
		_cache.Remove(AllPlayersCacheKey);
	}
}

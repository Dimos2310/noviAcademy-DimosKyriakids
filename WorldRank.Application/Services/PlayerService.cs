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
	private readonly ICache _cache;
	private readonly ILogger<PlayerService> _logger;

	public PlayerService(IPlayerRepository playerRepository, ICache cache, ILogger<PlayerService> logger)
	{
		_playerRepository = playerRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<Player> AddPlayerAsync(string name, int score, CancellationToken cancellationToken = default)
	{
		// The id is assigned by the store (database identity or the in-memory
		// repository), so the service never generates one itself.
		var player = new Player(name);
		player.AddScore(score);
		await _playerRepository.AddPlayerAsync(player, cancellationToken);

		// Write-through: the cached list is now stale, drop it so the next read reloads it.
		_cache.Remove(AllPlayersCacheKey);

		return player;
	}

	public async Task<IReadOnlyList<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
	{
		if (_cache.TryGetValue(AllPlayersCacheKey, out IReadOnlyList<Player>? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT all players");
			return cached;
		}

		_logger.LogInformation("Cache MISS all players — loading from database");
		var players = await _playerRepository.GetAllPlayersAsync(cancellationToken);

		_cache.Set(AllPlayersCacheKey, players, CacheTtl);

		return players;
	}

	public async Task<IEnumerable<IGrouping<int, Player>>> GroupPlayersByScoreAsync(CancellationToken cancellationToken = default)
	{
		return await _playerRepository.GroupPlayersByScoreAsync(cancellationToken);
	}

	public async Task<Player?> FindPlayerByNameAsync(string name, CancellationToken cancellationToken = default)
	{
		var players = await GetAllPlayersAsync(cancellationToken);
		return players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<Player?> FindPlayerByIdAsync(int playerId, CancellationToken cancellationToken = default)
	{
		return await _playerRepository.FindPlayerAsync(playerId, cancellationToken);
	}

	public async Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		await _playerRepository.DeletePlayerAsync(playerId, cancellationToken);

		// Write-through: the cached list is now stale, drop it so the next read reloads it.
		_cache.Remove(AllPlayersCacheKey);
	}
}

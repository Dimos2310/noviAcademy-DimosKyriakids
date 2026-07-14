using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IPlayerRepository: owns every cache-aside read and write-through/
// invalidation write for players, so Commands/Queries stay pure business logic.
public class PlayerRepositoryCachingDecorator : IPlayerRepository
{
	private const string AllPlayersCacheKey = "AllPlayersKey";
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string PlayerCacheKey(int id) => $"Player:{id}";

	private readonly IPlayerRepository _inner;
	private readonly ICache _cache;
	private readonly ILogger<PlayerRepositoryCachingDecorator> _logger;

	public PlayerRepositoryCachingDecorator(IPlayerRepository inner, ICache cache, ILogger<PlayerRepositoryCachingDecorator> logger)
	{
		_inner = inner;
		_cache = cache;
		_logger = logger;
	}

	public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
	{
		await _inner.AddPlayerAsync(player, cancellationToken);

		_cache.Set(PlayerCacheKey(player.Id), player, CacheTtl);
		_cache.Remove(AllPlayersCacheKey);
	}

	public async Task<IReadOnlyList<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
	{
		if (_cache.TryGetValue(AllPlayersCacheKey, out IReadOnlyList<Player>? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT all players");
			return cached;
		}

		_logger.LogInformation("Cache MISS all players — loading from database");
		var players = await _inner.GetAllPlayersAsync(cancellationToken);

		_cache.Set(AllPlayersCacheKey, players, CacheTtl);
		return players;
	}

	public async Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		await _inner.DeletePlayerAsync(playerId, cancellationToken);

		_cache.Remove(PlayerCacheKey(playerId));
		_cache.Remove(AllPlayersCacheKey);
	}

	public async Task<Player?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		if (_cache.TryGetValue(PlayerCacheKey(playerId), out Player? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT player {PlayerId}", playerId);
			return cached;
		}

		_logger.LogInformation("Cache MISS player {PlayerId} — loading from database", playerId);
		var player = await _inner.FindPlayerAsync(playerId, cancellationToken);

		if (player is not null)
			_cache.Set(PlayerCacheKey(playerId), player, CacheTtl);

		return player;
	}

	// Not cached — matches the previous behaviour in PlayerService.
	public Task<IEnumerable<IGrouping<int, Player>>> GroupPlayersByScoreAsync(CancellationToken cancellationToken = default)
		=> _inner.GroupPlayersByScoreAsync(cancellationToken);
}

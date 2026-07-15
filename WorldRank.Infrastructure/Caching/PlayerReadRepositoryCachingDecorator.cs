using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IPlayerReadRepository: owns every cache-aside read for players, so
// Queries stay pure business logic.
public class PlayerReadRepositoryCachingDecorator : IPlayerReadRepository
{
	private const string AllPlayersCacheKey = "AllPlayersKey";
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string PlayerCacheKey(int id) => $"Player:{id}";

	private readonly IPlayerReadRepository _inner;
	private readonly ICache _cache;
	private readonly ILogger<PlayerReadRepositoryCachingDecorator> _logger;

	public PlayerReadRepositoryCachingDecorator(IPlayerReadRepository inner, ICache cache, ILogger<PlayerReadRepositoryCachingDecorator> logger)
	{
		_inner = inner;
		_cache = cache;
		_logger = logger;
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

using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IPlayerWriteRepository: owns every write-through/invalidation write
// for players, so Commands stay pure business logic.
public class PlayerWriteRepositoryCachingDecorator : IPlayerWriteRepository
{
	private const string AllPlayersCacheKey = "AllPlayersKey";
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string PlayerCacheKey(int id) => $"Player:{id}";

	private readonly IPlayerWriteRepository _inner;
	private readonly ICache _cache;

	public PlayerWriteRepositoryCachingDecorator(IPlayerWriteRepository inner, ICache cache)
	{
		_inner = inner;
		_cache = cache;
	}

	public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
	{
		await _inner.AddPlayerAsync(player, cancellationToken);

		_cache.Set(PlayerCacheKey(player.Id), player, CacheTtl);
		_cache.Remove(AllPlayersCacheKey);
	}

	public async Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		await _inner.DeletePlayerAsync(playerId, cancellationToken);

		_cache.Remove(PlayerCacheKey(playerId));
		_cache.Remove(AllPlayersCacheKey);
	}
}

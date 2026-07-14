using Microsoft.Extensions.Caching.Memory;
using WorldRank.Application.Interfaces;

namespace WorldRank.Infrastructure.Caching;

// Adapts IMemoryCache to the Application-owned ICache port, so services never
// take a dependency on a specific caching library.
public class MemoryCacheStore : ICache
{
	private readonly IMemoryCache _cache;

	public MemoryCacheStore(IMemoryCache cache)
	{
		_cache = cache;
	}

	public bool TryGetValue<T>(string key, out T? value) => _cache.TryGetValue(key, out value);

	public void Set<T>(string key, T value, TimeSpan ttl) => _cache.Set(key, value, ttl);

	public void Remove(string key) => _cache.Remove(key);
}

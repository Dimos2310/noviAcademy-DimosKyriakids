namespace WorldRank.Application.Interfaces;

// Cache port owned by the Application layer: services depend on this, never on a
// concrete caching library. Infrastructure supplies the implementation.
public interface ICache
{
	bool TryGetValue<T>(string key, out T? value);

	void Set<T>(string key, T value, TimeSpan ttl);

	void Remove(string key);
}

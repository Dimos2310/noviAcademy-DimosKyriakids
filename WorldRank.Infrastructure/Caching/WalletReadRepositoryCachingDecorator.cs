using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IWalletReadRepository: owns every cache-aside read for wallets, so
// Queries stay pure business logic.
public class WalletReadRepositoryCachingDecorator : IWalletReadRepository
{
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string WalletCacheKey(int id) => $"Wallet:{id}";
	private static string PlayerWalletsCacheKey(int playerId) => $"PlayerWallets:{playerId}";

	private readonly IWalletReadRepository _inner;
	private readonly ICache _cache;
	private readonly ILogger<WalletReadRepositoryCachingDecorator> _logger;

	public WalletReadRepositoryCachingDecorator(IWalletReadRepository inner, ICache cache, ILogger<WalletReadRepositoryCachingDecorator> logger)
	{
		_inner = inner;
		_cache = cache;
		_logger = logger;
	}

	// Not cached — no service ever exposed a "get every wallet" read.
	public Task<Wallet[]> GetAllAsync(CancellationToken cancellationToken = default)
		=> _inner.GetAllAsync(cancellationToken);

	public async Task<List<Wallet>> GetAllWalletsByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default)
	{
		var key = PlayerWalletsCacheKey(playerId);

		if (_cache.TryGetValue(key, out List<Wallet>? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT wallets for player {PlayerId}", playerId);
			return cached;
		}

		_logger.LogInformation("Cache MISS wallets for player {PlayerId} — loading from database", playerId);
		var wallets = await _inner.GetAllWalletsByPlayerIdAsync(playerId, cancellationToken);

		_cache.Set(key, wallets, CacheTtl);
		return wallets;
	}

	// Not cached — composite-key lookup only used to translate (playerId, currency) into a wallet id.
	public Task<Wallet> GetWalletAsync(int playerId, Currency currency, CancellationToken cancellationToken = default)
		=> _inner.GetWalletAsync(playerId, currency, cancellationToken);

	public async Task<Wallet?> GetByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var key = WalletCacheKey(walletId);

		if (_cache.TryGetValue(key, out Wallet? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT wallet {WalletId}", walletId);
			return cached;
		}

		_logger.LogInformation("Cache MISS wallet {WalletId} — loading from database", walletId);
		var wallet = await _inner.GetByIdAsync(walletId, cancellationToken);

		if (wallet is not null)
			_cache.Set(key, wallet, CacheTtl);

		return wallet;
	}
}

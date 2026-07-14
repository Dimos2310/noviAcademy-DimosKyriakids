using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IWalletRepository: owns every cache-aside read and write-through/
// invalidation write for wallets, so Commands/Queries stay pure business logic.
public class WalletRepositoryCachingDecorator : IWalletRepository
{
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string WalletCacheKey(int id) => $"Wallet:{id}";
	private static string PlayerWalletsCacheKey(int playerId) => $"PlayerWallets:{playerId}";

	private readonly IWalletRepository _inner;
	private readonly ICache _cache;
	private readonly ILogger<WalletRepositoryCachingDecorator> _logger;

	public WalletRepositoryCachingDecorator(IWalletRepository inner, ICache cache, ILogger<WalletRepositoryCachingDecorator> logger)
	{
		_inner = inner;
		_cache = cache;
		_logger = logger;
	}

	public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
	{
		await _inner.AddAsync(wallet, cancellationToken);
		Refresh(wallet);
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

	public async Task<Wallet> UpdateBalanceAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.UpdateBalanceAsync(walletId, newBalance, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	public async Task<Wallet> DepositAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.DepositAsync(walletId, amount, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	public async Task<Wallet> WithdrawAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.WithdrawAsync(walletId, amount, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	public async Task<Wallet> BlockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.BlockAsync(walletId, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	public async Task<Wallet> UnblockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.UnblockAsync(walletId, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	public async Task<Wallet> ApplyStrategyAsync(int walletId, IFundsStrategy strategy, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _inner.ApplyStrategyAsync(walletId, strategy, amount, cancellationToken);
		Refresh(wallet);
		return wallet;
	}

	// Write-through the single wallet entry, invalidate the player's wallet-list cache.
	private void Refresh(Wallet wallet)
	{
		_cache.Set(WalletCacheKey(wallet.Id), wallet, CacheTtl);
		_cache.Remove(PlayerWalletsCacheKey(wallet.PlayerId));
	}
}

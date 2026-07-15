using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Caching;

// Decorator over IWalletWriteRepository: owns every write-through/invalidation write
// for wallets, so Commands stay pure business logic.
public class WalletWriteRepositoryCachingDecorator : IWalletWriteRepository
{
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private static string WalletCacheKey(int id) => $"Wallet:{id}";
	private static string PlayerWalletsCacheKey(int playerId) => $"PlayerWallets:{playerId}";

	private readonly IWalletWriteRepository _inner;
	private readonly ICache _cache;

	public WalletWriteRepositoryCachingDecorator(IWalletWriteRepository inner, ICache cache)
	{
		_inner = inner;
		_cache = cache;
	}

	public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
	{
		await _inner.AddAsync(wallet, cancellationToken);
		Refresh(wallet);
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

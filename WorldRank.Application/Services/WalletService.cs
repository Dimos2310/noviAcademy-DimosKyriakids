using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Application.Services;

// Use-case logic only. No console / presentation concerns: inputs come in as
// parameters, failures surface as domain exceptions for the caller to handle.
public class WalletService : IWalletService
{
	private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

	private readonly IWalletReadRepository _walletReadRepository;
	private readonly IWalletWriteRepository _walletWriteRepository;
	private readonly IPlayerReadRepository _playerReadRepository;
	private readonly IReadOnlyDictionary<FundsOperation, IFundsStrategy> _fundsStrategies;
	private readonly ICache _cache;
	private readonly ILogger<WalletService> _logger;

	public WalletService(
		IWalletReadRepository walletReadRepository,
		IWalletWriteRepository walletWriteRepository,
		IPlayerReadRepository playerReadRepository,
		IEnumerable<IFundsStrategy> strategies,
		ICache cache,
		ILogger<WalletService> logger)
	{
		_walletReadRepository = walletReadRepository;
		_walletWriteRepository = walletWriteRepository;
		_playerReadRepository = playerReadRepository;

		// Index every registered strategy by the operation it implements.
		_fundsStrategies = strategies.ToDictionary(strategy => strategy.Operation);

		_cache = cache;
		_logger = logger;
	}

	private static string WalletCacheKey(int walletId) => $"Wallet:{walletId}";

	private static string PlayerWalletsCacheKey(int playerId) => $"PlayerWallets:{playerId}";

	public async Task<Wallet> AddWalletToPlayerAsync(int playerId, Currency currency, decimal initialBalance, CancellationToken cancellationToken = default)
	{
		if (await _playerReadRepository.FindPlayerAsync(playerId, cancellationToken) is null)
			throw new PlayerNotFoundException(playerId);

		// No id here: the store (database identity or in-memory repository) assigns it.
		var wallet = new Wallet(playerId, currency, initialBalance);
		await _walletWriteRepository.AddAsync(wallet, cancellationToken);

		// Write-through + list-cache invalidation for the player this wallet belongs to.
		_cache.Set(WalletCacheKey(wallet.Id), wallet, CacheTtl);
		_cache.Remove(PlayerWalletsCacheKey(playerId));

		return wallet;
	}

	public async Task<IReadOnlyList<Wallet>> GetWalletsOfPlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		var key = PlayerWalletsCacheKey(playerId);

		if (_cache.TryGetValue(key, out IReadOnlyList<Wallet>? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT wallets for player {PlayerId}", playerId);
			return cached;
		}

		_logger.LogInformation("Cache MISS wallets for player {PlayerId} — loading from database", playerId);
		var wallets = await _walletReadRepository.GetAllWalletsByPlayerIdAsync(playerId, cancellationToken);

		_cache.Set(key, (IReadOnlyList<Wallet>)wallets, CacheTtl);

		return wallets;
	}

	public async Task<Wallet?> GetWalletByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var key = WalletCacheKey(walletId);

		if (_cache.TryGetValue(key, out Wallet? cached) && cached is not null)
		{
			_logger.LogInformation("Cache HIT wallet {WalletId}", walletId);
			return cached;
		}

		_logger.LogInformation("Cache MISS wallet {WalletId} — loading from database", walletId);
		var wallet = await _walletReadRepository.GetByIdAsync(walletId, cancellationToken);

		if (wallet is not null)
			_cache.Set(key, wallet, CacheTtl);

		return wallet;
	}

	public async Task DepositAsync(int playerId, Currency currency, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await DepositByIdAsync(wallet.Id, amount, cancellationToken);
	}

	public async Task WithdrawAsync(int playerId, Currency currency, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await WithdrawByIdAsync(wallet.Id, amount, cancellationToken);
	}

	public async Task BlockAsync(int playerId, Currency currency, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await BlockByIdAsync(wallet.Id, cancellationToken);
	}

	public async Task UnblockAsync(int playerId, Currency currency, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await UnblockByIdAsync(wallet.Id, cancellationToken);
	}

	public async Task UpdateBalanceAsync(int playerId, Currency currency, decimal newBalance, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await UpdateBalanceByIdAsync(wallet.Id, newBalance, cancellationToken);
	}

	public async Task ApplyFundsAsync(int playerId, Currency currency, FundsOperation operation, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletReadRepository.GetWalletAsync(playerId, currency, cancellationToken);
		await ApplyFundsByIdAsync(wallet.Id, operation, amount, cancellationToken);
	}

	public async Task<Wallet> DepositByIdAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletWriteRepository.DepositAsync(walletId, amount, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	public async Task<Wallet> WithdrawByIdAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletWriteRepository.WithdrawAsync(walletId, amount, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	public async Task<Wallet> BlockByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletWriteRepository.BlockAsync(walletId, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	public async Task<Wallet> UnblockByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletWriteRepository.UnblockAsync(walletId, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	public async Task<Wallet> UpdateBalanceByIdAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default)
	{
		var wallet = await _walletWriteRepository.UpdateBalanceAsync(walletId, newBalance, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	public async Task<Wallet> ApplyFundsByIdAsync(int walletId, FundsOperation operation, decimal amount, CancellationToken cancellationToken = default)
	{
		// Pick the strategy that matches the chosen operation (resolved from DI, no factory).
		var strategy = _fundsStrategies[operation];

		var wallet = await _walletWriteRepository.ApplyStrategyAsync(walletId, strategy, amount, cancellationToken);
		RefreshWalletCache(wallet);
		return wallet;
	}

	// Write-through the single wallet entry with its fresh value, and invalidate the
	// player's wallet-list cache since one of its entries just changed.
	private void RefreshWalletCache(Wallet wallet)
	{
		_cache.Set(WalletCacheKey(wallet.Id), wallet, CacheTtl);
		_cache.Remove(PlayerWalletsCacheKey(wallet.PlayerId));
	}
}

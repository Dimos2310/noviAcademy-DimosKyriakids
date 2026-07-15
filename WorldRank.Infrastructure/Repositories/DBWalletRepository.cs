using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;
using WorldRank.Infrastructure.Persistence.Context;

namespace WorldRank.Infrastructure.Repositories;

public class DBWalletRepository : IWalletReadRepository, IWalletWriteRepository
{
	private const int MaxConcurrencyRetries = 3;

	private readonly WorldRankDbContext _context;
	private readonly ILogger<DBWalletRepository> _logger;

	public DBWalletRepository(WorldRankDbContext context, ILogger<DBWalletRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
	{
		var exists = await _context.Wallets.AnyAsync(item => item.PlayerId == wallet.PlayerId && item.Currency == wallet.Currency, cancellationToken);

		if (exists)
		{
			throw new DuplicateWalletException(wallet.PlayerId, wallet.Currency);
		}

		_context.Wallets.Add(wallet);
		await _context.SaveChangesAsync(cancellationToken);
		_logger.LogInformation("Wallet created in database for player {PlayerId} in {Currency} with balance {Balance}", wallet.PlayerId, wallet.Currency, wallet.Balance);
	}

	public async Task<Wallet[]> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Wallets.AsNoTracking().ToArrayAsync(cancellationToken);
	}

	public async Task<List<Wallet>> GetAllWalletsByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default)
	{
		return await _context.Wallets.AsNoTracking().Where(item => item.PlayerId == playerId).ToListAsync(cancellationToken);
	}

	public async Task<Wallet> GetWalletAsync(int playerId, Currency currency, CancellationToken cancellationToken = default)
	{
		var wallet = await _context.Wallets.AsNoTracking()
			.SingleOrDefaultAsync(item => item.PlayerId == playerId && item.Currency == currency, cancellationToken);

		if (wallet is null)
		{
			throw new WalletNotFoundException(playerId, currency);
		}

		return wallet;
	}

	public async Task<Wallet?> GetByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		return await _context.Wallets.AsNoTracking().FirstOrDefaultAsync(item => item.Id == walletId, cancellationToken);
	}

	public async Task<Wallet> UpdateBalanceAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => w.SetBalance(newBalance), cancellationToken);
		_logger.LogInformation("Wallet {WalletId} balance set in database to {Balance}", walletId, newBalance);
		return wallet;
	}

	public async Task<Wallet> DepositAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => w.Deposit(amount), cancellationToken);
		_logger.LogInformation("Deposited {Amount} to wallet {WalletId} in database (balance {Balance})", amount, walletId, wallet.Balance);
		return wallet;
	}

	public async Task<Wallet> WithdrawAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => w.Withdraw(amount), cancellationToken);
		_logger.LogInformation("Withdrew {Amount} from wallet {WalletId} in database (balance {Balance})", amount, walletId, wallet.Balance);
		return wallet;
	}

	public async Task<Wallet> BlockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => w.Block(), cancellationToken);
		_logger.LogInformation("Wallet {WalletId} blocked in database", walletId);
		return wallet;
	}

	public async Task<Wallet> UnblockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => w.Unblock(), cancellationToken);
		_logger.LogInformation("Wallet {WalletId} unblocked in database", walletId);
		return wallet;
	}

	public async Task<Wallet> ApplyStrategyAsync(int walletId, IFundsStrategy strategy, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = await ExecuteWithOptimisticRetryAsync(walletId, w => strategy.Execute(w, amount), cancellationToken);
		_logger.LogInformation("Applied {Strategy} of {Amount} to wallet {WalletId} in database (balance {Balance})",
			strategy.GetType().Name, amount, walletId, wallet.Balance);
		return wallet;
	}

	private async Task<Wallet> GetTrackedWalletAsync(int walletId, CancellationToken cancellationToken)
	{
		var wallet = await _context.Wallets.FirstOrDefaultAsync(item => item.Id == walletId, cancellationToken);

		if (wallet is null)
		{
			throw new WalletNotFoundByIdException(walletId);
		}

		return wallet;
	}

	// Read-modify-write under a rowversion concurrency token: if another request saved
	// changes to this wallet between our read and our SaveChanges, EF Core throws
	// DbUpdateConcurrencyException instead of silently overwriting that update. We
	// re-fetch the now-current row and reapply the mutation on top of it, so two
	// concurrent deposits both land instead of the second one clobbering the first.
	private async Task<Wallet> ExecuteWithOptimisticRetryAsync(int walletId, Action<Wallet> mutate, CancellationToken cancellationToken)
	{
		for (var attempt = 1; ; attempt++)
		{
			var wallet = await GetTrackedWalletAsync(walletId, cancellationToken);
			mutate(wallet);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);
				return wallet;
			}
			catch (DbUpdateConcurrencyException) when (attempt < MaxConcurrencyRetries)
			{
				_context.Entry(wallet).State = EntityState.Detached;
			}
		}
	}
}

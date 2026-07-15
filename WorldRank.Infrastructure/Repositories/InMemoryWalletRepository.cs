using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Infrastructure.Repositories;

public class InMemoryWalletRepository : IWalletReadRepository, IWalletWriteRepository
{
	private readonly ILogger<InMemoryWalletRepository> _logger;

	private readonly List<Wallet> _wallets = new();

	public InMemoryWalletRepository(ILogger<InMemoryWalletRepository> logger)
	{
		_logger = logger;
	}

	public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
	{
		var exists = _wallets.Any(item => item.PlayerId == wallet.PlayerId && item.Currency == wallet.Currency);

		if (exists)
		{
			throw new DuplicateWalletException(wallet.PlayerId, wallet.Currency);
		}

		// Assign the next id here: the store owns id generation, not the service.
		var id = _wallets.Count == 0 ? 1 : _wallets.Max(item => item.Id) + 1;
		var stored = new Wallet(id, wallet.PlayerId, wallet.Currency, wallet.Balance, wallet.IsBlocked);

		_wallets.Add(stored);
		_logger.LogInformation("Wallet created for player {PlayerId} in {Currency} with balance {Balance}", stored.PlayerId, stored.Currency, stored.Balance);

		return Task.CompletedTask;
	}

	public Task<Wallet[]> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_wallets.ToArray());
	}

	public Task<List<Wallet>> GetAllWalletsByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_wallets.Where(item => item.PlayerId == playerId).ToList());
	}

	public Task<Wallet> GetWalletAsync(int playerId, Currency currency, CancellationToken cancellationToken = default)
	{
		var wallet = _wallets.SingleOrDefault(item => item.PlayerId == playerId && item.Currency == currency);

		if (wallet is null)
		{
			throw new WalletNotFoundException(playerId, currency);
		}

		return Task.FromResult(wallet);
	}

	public Task<Wallet?> GetByIdAsync(int walletId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_wallets.FirstOrDefault(item => item.Id == walletId));
	}

	public Task<Wallet> UpdateBalanceAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		wallet.SetBalance(newBalance);
		_logger.LogInformation("Wallet {WalletId} balance set to {Balance}", walletId, newBalance);
		return Task.FromResult(wallet);
	}

	public Task<Wallet> DepositAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		wallet.Deposit(amount);
		_logger.LogInformation("Deposited {Amount} to wallet {WalletId} (balance {Balance})", amount, walletId, wallet.Balance);
		return Task.FromResult(wallet);
	}

	public Task<Wallet> WithdrawAsync(int walletId, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		wallet.Withdraw(amount);
		_logger.LogInformation("Withdrew {Amount} from wallet {WalletId} (balance {Balance})", amount, walletId, wallet.Balance);
		return Task.FromResult(wallet);
	}

	public Task<Wallet> BlockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		wallet.Block();
		_logger.LogInformation("Wallet {WalletId} blocked", walletId);
		return Task.FromResult(wallet);
	}

	public Task<Wallet> UnblockAsync(int walletId, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		wallet.Unblock();
		_logger.LogInformation("Wallet {WalletId} unblocked", walletId);
		return Task.FromResult(wallet);
	}

	public Task<Wallet> ApplyStrategyAsync(int walletId, IFundsStrategy strategy, decimal amount, CancellationToken cancellationToken = default)
	{
		var wallet = GetTrackedWallet(walletId);
		strategy.Execute(wallet, amount);
		_logger.LogInformation("Applied {Strategy} of {Amount} to wallet {WalletId} (balance {Balance})",
			strategy.GetType().Name, amount, walletId, wallet.Balance);
		return Task.FromResult(wallet);
	}

	private Wallet GetTrackedWallet(int walletId)
	{
		var wallet = _wallets.FirstOrDefault(item => item.Id == walletId);

		if (wallet is null)
		{
			throw new WalletNotFoundByIdException(walletId);
		}

		return wallet;
	}
}

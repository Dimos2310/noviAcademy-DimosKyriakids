using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletRepository
{
	Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

	Task<Wallet[]> GetAllAsync(CancellationToken cancellationToken = default);

	Task<List<Wallet>> GetAllWalletsByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default);

	// Composite-key lookup — used to translate (playerId, currency) into a wallet id.
	Task<Wallet> GetWalletAsync(int playerId, Currency currency, CancellationToken cancellationToken = default);

	// Identity lookup — the canonical way the API addresses a wallet.
	Task<Wallet?> GetByIdAsync(int walletId, CancellationToken cancellationToken = default);

	Task<Wallet> UpdateBalanceAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default);

	Task<Wallet> DepositAsync(int walletId, decimal amount, CancellationToken cancellationToken = default);

	Task<Wallet> WithdrawAsync(int walletId, decimal amount, CancellationToken cancellationToken = default);

	Task<Wallet> BlockAsync(int walletId, CancellationToken cancellationToken = default);

	Task<Wallet> UnblockAsync(int walletId, CancellationToken cancellationToken = default);

	// Applies a funds strategy to a wallet and persists the result in one step,
	// so every mutating operation self-saves (no separate Save() to remember).
	Task<Wallet> ApplyStrategyAsync(int walletId, IFundsStrategy strategy, decimal amount, CancellationToken cancellationToken = default);
}

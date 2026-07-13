using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletService
{
	Task<Wallet> AddWalletToPlayerAsync(int playerId, Currency currency, decimal initialBalance, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Wallet>> GetWalletsOfPlayerAsync(int playerId, CancellationToken cancellationToken = default);

	// Identity-based read — the shape the API's GET /wallets/{id} uses.
	Task<Wallet?> GetWalletByIdAsync(int walletId, CancellationToken cancellationToken = default);

	// Composite-key (playerId, currency) operations — used by the console menu.
	Task DepositAsync(int playerId, Currency currency, decimal amount, CancellationToken cancellationToken = default);

	Task WithdrawAsync(int playerId, Currency currency, decimal amount, CancellationToken cancellationToken = default);

	Task BlockAsync(int playerId, Currency currency, CancellationToken cancellationToken = default);

	Task UnblockAsync(int playerId, Currency currency, CancellationToken cancellationToken = default);

	Task UpdateBalanceAsync(int playerId, Currency currency, decimal newBalance, CancellationToken cancellationToken = default);

	Task ApplyFundsAsync(int playerId, Currency currency, FundsOperation operation, decimal amount, CancellationToken cancellationToken = default);

	// Identity-based operations — the shape the API's POST /wallets/{id}/... routes use.
	Task<Wallet> DepositByIdAsync(int walletId, decimal amount, CancellationToken cancellationToken = default);

	Task<Wallet> WithdrawByIdAsync(int walletId, decimal amount, CancellationToken cancellationToken = default);

	Task<Wallet> BlockByIdAsync(int walletId, CancellationToken cancellationToken = default);

	Task<Wallet> UnblockByIdAsync(int walletId, CancellationToken cancellationToken = default);

	Task<Wallet> UpdateBalanceByIdAsync(int walletId, decimal newBalance, CancellationToken cancellationToken = default);

	Task<Wallet> ApplyFundsByIdAsync(int walletId, FundsOperation operation, decimal amount, CancellationToken cancellationToken = default);
}

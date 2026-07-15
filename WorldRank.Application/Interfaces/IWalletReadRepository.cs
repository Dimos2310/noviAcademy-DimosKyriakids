using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Interfaces;

public interface IWalletReadRepository
{
	Task<Wallet[]> GetAllAsync(CancellationToken cancellationToken = default);

	Task<List<Wallet>> GetAllWalletsByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default);

	// Composite-key lookup — used to translate (playerId, currency) into a wallet id.
	Task<Wallet> GetWalletAsync(int playerId, Currency currency, CancellationToken cancellationToken = default);

	// Identity lookup — the canonical way the API addresses a wallet.
	Task<Wallet?> GetByIdAsync(int walletId, CancellationToken cancellationToken = default);
}

using WorldRank.Domain.Entities;

namespace WorldRank.Api.Contracts;

// Response DTO — the shape the client receives. Never expose the domain entity directly.
public record WalletResponse(int Id, int PlayerId, string Currency, decimal Balance, bool IsBlocked)
{
	public static WalletResponse From(Wallet wallet) =>
		new(wallet.Id, wallet.PlayerId, wallet.Currency.ToString(), wallet.Balance, wallet.IsBlocked);
}

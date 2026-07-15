using MediatR;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Queries.Wallets;

// Resolves a wallet id from the (playerId, currency) composite key — used by the console
// menu, which prompts for player + currency rather than a wallet id directly.
public record GetWalletByPlayerAndCurrencyQuery(int PlayerId, Currency Currency) : IRequest<Wallet>;

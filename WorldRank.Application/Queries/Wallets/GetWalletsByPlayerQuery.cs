using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public record GetWalletsByPlayerQuery(int PlayerId) : IRequest<IReadOnlyList<Wallet>>;

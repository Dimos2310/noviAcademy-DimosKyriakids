using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Wallets;

public record GetWalletByIdQuery(int WalletId) : IRequest<Wallet?>;

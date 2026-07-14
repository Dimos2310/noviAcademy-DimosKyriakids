using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public record UnblockWalletCommand(int WalletId) : IRequest<Wallet>;

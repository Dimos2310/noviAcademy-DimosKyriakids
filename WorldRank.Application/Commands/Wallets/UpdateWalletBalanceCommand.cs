using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public record UpdateWalletBalanceCommand(int WalletId, decimal NewBalance) : IRequest<Wallet>;

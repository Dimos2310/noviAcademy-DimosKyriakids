using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public record BlockWalletCommand(int WalletId) : IRequest<Wallet>;

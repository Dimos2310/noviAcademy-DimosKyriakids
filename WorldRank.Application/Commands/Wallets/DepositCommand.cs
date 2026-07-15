using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public record DepositCommand(int WalletId, decimal Amount) : IRequest<Wallet>;

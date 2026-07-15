using MediatR;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Wallets;

public record ApplyFundsCommand(int WalletId, FundsOperation Operation, decimal Amount) : IRequest<Wallet>;

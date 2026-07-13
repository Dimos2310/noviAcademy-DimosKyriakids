using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRank.Api.Contracts;

public record AddWalletRequest(int PlayerId, Currency Currency, decimal InitialBalance);

public record WalletAmountRequest(decimal Amount);

public record UpdateWalletBalanceRequest(decimal NewBalance);

public record ApplyFundsRequest(FundsOperation Operation, decimal Amount);

using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRand.API.Contracts;

public record AddWalletRequest(Currency Currency, decimal InitialBalance);

public record WalletAmountRequest(decimal Amount);

public record UpdateWalletBalanceRequest(decimal NewBalance);

public record ApplyFundsRequest(FundsOperation Operation, decimal Amount);

using System;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRank.Application.Services;

// Small shared helpers for reading and validating console input.
public static class Prompts
{
	public static int? PromptPlayerId()
	{
		Console.Write("Give player id: ");
		if (int.TryParse(Console.ReadLine(), out var playerId))
			return playerId;

		Console.WriteLine("Player id must be a whole number.");
		return null;
	}

	public static Currency? PromptCurrency()
	{
		Console.Write("Give Currency: 1 - EUR | 2 - USD\n");
		switch (Console.ReadLine())
		{
			case "1":
				return Currency.EUR;
			case "2":
				return Currency.USD;
			default:
				Console.WriteLine("Unknown currency.");
				return null;
		}
	}

	public static decimal? PromptAmount(string label)
	{
		Console.Write($"{label}: ");
		if (decimal.TryParse(Console.ReadLine(), out var amount))
			return amount;

		Console.WriteLine("Amount must be a number.");
		return null;
	}

	public static FundsOperation? PromptFundsOperation()
	{
		Console.Write("Operation: 1 - Add | 2 - Subtract | 3 - Force subtract (penalty)\n");
		switch (Console.ReadLine())
		{
			case "1":
				return FundsOperation.Add;
			case "2":
				return FundsOperation.Subtract;
			case "3":
				return FundsOperation.ForceSubtract;
			default:
				Console.WriteLine("Unknown operation.");
				return null;
		}
	}
}

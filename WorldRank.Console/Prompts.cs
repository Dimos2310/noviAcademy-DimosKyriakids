using WorldRank.Application.Strategies;
using WorldRank.Domain.Enums;

namespace WorldRank.Console;

// Presentation helpers: reading and validating console input. These live in the
// Console (presentation) project, not in Application - the use-case services stay
// free of any I/O.
public static class Prompts
{
	public static int? PromptPlayerId()
	{
		System.Console.Write("Give player id: ");
		if (int.TryParse(System.Console.ReadLine(), out var playerId))
			return playerId;

		System.Console.WriteLine("Player id must be a whole number.");
		return null;
	}

	public static Currency? PromptCurrency()
	{
		System.Console.Write("Give Currency: 1 - EUR | 2 - USD\n");
		switch (System.Console.ReadLine())
		{
			case "1":
				return Currency.EUR;
			case "2":
				return Currency.USD;
			default:
				System.Console.WriteLine("Unknown currency.");
				return null;
		}
	}

	public static decimal? PromptAmount(string label)
	{
		System.Console.Write($"{label}: ");
		if (decimal.TryParse(System.Console.ReadLine(), out var amount))
			return amount;

		System.Console.WriteLine("Amount must be a number.");
		return null;
	}

	public static FundsOperation? PromptFundsOperation()
	{
		System.Console.Write("Operation: 1 - Add | 2 - Subtract | 3 - Force subtract (penalty)\n");
		switch (System.Console.ReadLine())
		{
			case "1":
				return FundsOperation.Add;
			case "2":
				return FundsOperation.Subtract;
			case "3":
				return FundsOperation.ForceSubtract;
			default:
				System.Console.WriteLine("Unknown operation.");
				return null;
		}
	}
}

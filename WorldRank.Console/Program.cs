using Microsoft.Extensions.DependencyInjection;
using NLog;
using WorldRank.Application.Services;
using WorldRank.Console;
using WorldRank.Domain.Exceptions;
using WorldRank.Infrastructure;

var logger = LogManager.GetCurrentClassLogger();

// Composition root: register every layer's services, then build the container.
var services = new ServiceCollection();
services.AddWorldRank();

using var provider = services.BuildServiceProvider();

// Create the database and its schema on first run (no-op when running in-memory).
provider.InitializeDatabase();

var playerService = provider.GetRequiredService<PlayerService>();
var walletService = provider.GetRequiredService<WalletService>();

logger.Info("Application started.");

while (true)
{
	Console.WriteLine("\n=== WorldRank Player Registry ===");
	Console.WriteLine("--- Players ---");
	Console.WriteLine("1. Add player");
	Console.WriteLine("2. List all players");
	Console.WriteLine("3. List players grouped by score");
	Console.WriteLine("4. Find player by name");
	Console.WriteLine("5. Find player by id");
	Console.WriteLine("6. Delete player");
	Console.WriteLine("--- Wallets ---");
	Console.WriteLine("7. Add wallet to player");
	Console.WriteLine("8. Show player wallets");
	Console.WriteLine("9. Deposit to wallet");
	Console.WriteLine("10. Withdraw from wallet");
	Console.WriteLine("11. Block wallet");
	Console.WriteLine("12. Unblock wallet");
	Console.WriteLine("13. Update wallet balance");
	Console.WriteLine("14. Apply funds operation (strategy)");
	Console.WriteLine("0. Exit");
	Console.Write("> ");

	Action? action = Console.ReadLine() switch
	{
		"1" => AddPlayer,
		"2" => ListPlayers,
		"3" => ListPlayersByScore,
		"4" => FindPlayerByName,
		"5" => FindPlayerById,
		"6" => DeletePlayer,
		"7" => AddWalletToPlayer,
		"8" => GetWalletsOfPlayer,
		"9" => DepositToWallet,
		"10" => WithdrawFromWallet,
		"11" => BlockWallet,
		"12" => UnblockWallet,
		"13" => UpdateWalletBalance,
		"14" => ApplyFundsOperation,
		"0" => null,
		_ => () => Console.WriteLine("Unknown option.")
	};

	if (action is null)
	{
		logger.Info("Application exiting.");
		LogManager.Shutdown(); // flush file writes before exit
		return; // "0" selected — exit
	}

	try
	{
		action();
	}
	catch (Exception ex)
	{
		// Safety net: log any exception the specific handlers did not catch, and keep the app running.
		logger.Error(ex, "Unexpected error while handling a menu action");
		Console.WriteLine($"Unexpected error: {ex.Message}");
	}
}

#region Player Methods

void AddPlayer()
{
	Console.Write("Name: ");
	var name = Console.ReadLine();
	if (string.IsNullOrWhiteSpace(name))
	{
		Console.WriteLine("Name cannot be empty.");
		return;
	}

	Console.Write("Score: ");
	if (!int.TryParse(Console.ReadLine(), out var score))
	{
		Console.WriteLine("Score must be a whole number.");
		return;
	}

	playerService.AddPlayer(name, score);
	Console.WriteLine("Player added successfully.");
}

void ListPlayers()
{
	var all = playerService.GetAllPlayers().ToList();

	if (all.Count == 0)
	{
		Console.WriteLine("No players registered.");
		return;
	}

	foreach (var player in all)
		Console.WriteLine(player);
}

void ListPlayersByScore()
{
	var groups = playerService.GroupPlayersByScore().ToList();

	if (groups.Count == 0)
	{
		Console.WriteLine("No players registered.");
		return;
	}

	foreach (var group in groups)
	{
		Console.WriteLine($"Score {group.Key}:");
		foreach (var player in group)
			Console.WriteLine($"  {player}");
	}
}

void FindPlayerByName()
{
	Console.Write("Search by name: ");
	var term = Console.ReadLine() ?? string.Empty;

	var player = playerService.FindPlayerByName(term);

	Console.WriteLine(player is null ? "No player found." : player.ToString());
}

void FindPlayerById()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var player = playerService.FindPlayerById(playerId.Value);

	Console.WriteLine(player is null ? "No player found." : player.ToString());
}

void DeletePlayer()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	playerService.DeletePlayer(playerId.Value);
	Console.WriteLine("Player deleted (if it existed).");
}

#endregion Player Methods

#region Wallet Methods

void AddWalletToPlayer()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	var balance = Prompts.PromptAmount("Initial balance");
	if (balance is null)
		return;

	try
	{
		walletService.AddWalletToPlayer(playerId.Value, currency.Value, balance.Value);
		Console.WriteLine("Wallet added successfully.");
	}
	catch (PlayerNotFoundException ex)
	{
		logger.Warn(ex, "Could not add wallet, player {PlayerId} not found", playerId);
		Console.WriteLine($"Error: {ex.Message}");
	}
	catch (WalletException ex)
	{
		logger.Warn(ex, "Could not add wallet for player {PlayerId} in {Currency}", playerId, currency);
		Console.WriteLine($"Error: {ex.Message}");
	}
}

void GetWalletsOfPlayer()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var wallets = walletService.GetWalletsOfPlayer(playerId.Value);

	if (wallets.Count == 0)
	{
		Console.WriteLine("No wallets found for this player.");
		return;
	}

	for (var i = 0; i < wallets.Count; i++)
		Console.WriteLine($"Wallet Number {i} {wallets[i]}");
}

void DepositToWallet()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	var amount = Prompts.PromptAmount("Amount to deposit");
	if (amount is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.Deposit(playerId.Value, currency.Value, amount.Value);
		Console.WriteLine("Deposit successful.");
	});
}

void WithdrawFromWallet()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	var amount = Prompts.PromptAmount("Amount to withdraw");
	if (amount is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.Withdraw(playerId.Value, currency.Value, amount.Value);
		Console.WriteLine("Withdrawal successful.");
	});
}

void BlockWallet()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.Block(playerId.Value, currency.Value);
		Console.WriteLine("Wallet blocked.");
	});
}

void UnblockWallet()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.Unblock(playerId.Value, currency.Value);
		Console.WriteLine("Wallet unblocked.");
	});
}

void UpdateWalletBalance()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	var newBalance = Prompts.PromptAmount("New balance");
	if (newBalance is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.UpdateBalance(playerId.Value, currency.Value, newBalance.Value);
		Console.WriteLine("Balance updated.");
	});
}

void ApplyFundsOperation()
{
	var playerId = Prompts.PromptPlayerId();
	if (playerId is null)
		return;

	var currency = Prompts.PromptCurrency();
	if (currency is null)
		return;

	var operation = Prompts.PromptFundsOperation();
	if (operation is null)
		return;

	var amount = Prompts.PromptAmount("Amount");
	if (amount is null)
		return;

	RunWalletOperation(() =>
	{
		walletService.ApplyFunds(playerId.Value, currency.Value, operation.Value, amount.Value);
		Console.WriteLine($"{operation} operation applied.");
	});
}

// Runs a wallet operation and turns any domain (WalletException) failure into a friendly message + log.
void RunWalletOperation(Action operation)
{
	try
	{
		operation();
	}
	catch (WalletException ex)
	{
		logger.Warn(ex, "Wallet operation failed");
		Console.WriteLine($"Error: {ex.Message}");
	}
}

#endregion Wallet Methods

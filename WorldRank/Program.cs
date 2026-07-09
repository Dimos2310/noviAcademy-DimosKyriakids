using NLog;
using WorldRank.Console;
using WorldRank.Console.Enums;
using WorldRank.Console.Exceptions;

var logger = LogManager.GetCurrentClassLogger();

//Wallets are stored in their own repository and reference the player via PlayerId
IWalletRepository walletRepository = new InMemoryWalletRepository();
IPlayerRepository playerRepository = new InMemoryPlayerRepository();

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

#region Input Helpers

int? PromptPlayerId()
{
	Console.Write("Give player id: ");
	if (int.TryParse(Console.ReadLine(), out var playerId))
		return playerId;

	Console.WriteLine("Player id must be a whole number.");
	return null;
}

Currency? PromptCurrency()
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

decimal? PromptAmount(string label)
{
	Console.Write($"{label}: ");
	if (decimal.TryParse(Console.ReadLine(), out var amount))
		return amount;

	Console.WriteLine("Amount must be a number.");
	return null;
}

// Generates a random, unique player id (avoids collisions with already-registered players).
int GeneratePlayerId()
{
	var existingIds = playerRepository.GetAllPlayers().Select(p => p.Id).ToHashSet();

	int id;
	do
	{
		id = Random.Shared.Next(1, int.MaxValue);
	}
	while (existingIds.Contains(id));

	return id;
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

#endregion Input Helpers

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
	var scoreInput = Console.ReadLine();
	if (!int.TryParse(scoreInput, out var score))
	{
		Console.WriteLine("Score must be a whole number.");
		return;
	}

	var player = new Player(GeneratePlayerId(), name);
	player.AddScore(score);
	playerRepository.AddPlayer(player);
	Console.WriteLine("Player added successfully.");
}

void ListPlayers()
{
	var all = playerRepository.GetAllPlayers().ToList();

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
	var groups = playerRepository.GroupPlayersByScore().ToList();

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

	var player = playerRepository.GetAllPlayers()
		.FirstOrDefault(p => p.Name.Equals(term, StringComparison.OrdinalIgnoreCase));

	Console.WriteLine(player is null ? "No player found." : player.ToString());
}

void FindPlayerById()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var player = playerRepository.FindPlayer(playerId.Value);

	Console.WriteLine(player is null ? "No player found." : player.ToString());
}

void DeletePlayer()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	playerRepository.DeletePlayer(playerId.Value);
	Console.WriteLine("Player deleted (if it existed).");
}

#endregion Player Methods

#region Wallet Methods

void AddWalletToPlayer()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	var balance = PromptAmount("Initial balance");
	if (balance is null)
		return;

	try
	{
		if (playerRepository.FindPlayer(playerId.Value) is null)
			throw new PlayerNotFoundException(playerId.Value);

		var wallet = new Wallet(playerId.Value, currency.Value, balance.Value);
		walletRepository.Add(wallet);
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
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var wallets = walletRepository.GetAllWalletsByPlayerId(playerId.Value);

	if (wallets.Count == 0)
	{
		Console.WriteLine("No wallets found for this player.");
		return;
	}

	foreach (var wallet in wallets)
		Console.WriteLine($"Wallet Number {wallets.IndexOf(wallet)} {wallet}");
}

void DepositToWallet()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	var amount = PromptAmount("Amount to deposit");
	if (amount is null)
		return;

	RunWalletOperation(() =>
	{
		walletRepository.Deposit(playerId.Value, currency.Value, amount.Value);
		Console.WriteLine("Deposit successful.");
	});
}

void WithdrawFromWallet()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	var amount = PromptAmount("Amount to withdraw");
	if (amount is null)
		return;

	RunWalletOperation(() =>
	{
		walletRepository.Withdraw(playerId.Value, currency.Value, amount.Value);
		Console.WriteLine("Withdrawal successful.");
	});
}

void BlockWallet()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	RunWalletOperation(() =>
	{
		walletRepository.Block(playerId.Value, currency.Value);
		Console.WriteLine("Wallet blocked.");
	});
}

void UnblockWallet()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	RunWalletOperation(() =>
	{
		walletRepository.Unblock(playerId.Value, currency.Value);
		Console.WriteLine("Wallet unblocked.");
	});
}

void UpdateWalletBalance()
{
	var playerId = PromptPlayerId();
	if (playerId is null)
		return;

	var currency = PromptCurrency();
	if (currency is null)
		return;

	var newBalance = PromptAmount("New balance");
	if (newBalance is null)
		return;

	RunWalletOperation(() =>
	{
		walletRepository.UpdateBalance(playerId.Value, currency.Value, newBalance.Value);
		Console.WriteLine("Balance updated.");
	});
}

#endregion Wallet Methods

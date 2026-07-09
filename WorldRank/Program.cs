using NLog;
using WorldRank;

var logger = LogManager.GetCurrentClassLogger();
logger.Info("App started");

// In-memory data stores: everything lives while the program runs.
// The wallet repository is created first so the player repository can
// cascade wallet deletion when a player is removed.
IWalletRepository walletRepository = new InMemoryWalletRepository();
IPlayerRepository playerRepository = new InMemoryPlayerRepository(walletRepository);

while (true)
{
    Console.WriteLine("========================================");
    Console.WriteLine("      WorldRank — Player Registry       ");
    Console.WriteLine("========================================");
    Console.WriteLine();
    Console.WriteLine("  1. Add player");
    Console.WriteLine("  2. List players");
    Console.WriteLine("  3. Find by name");
    Console.WriteLine("  4. Update score");
    Console.WriteLine("  5. Delete player");
    Console.WriteLine("  6. Group players by score");
    Console.WriteLine("  7. Add wallet to player");
    Console.WriteLine("  8. Deposit to wallet");
    Console.WriteLine("  9. Withdraw from wallet");
    Console.WriteLine(" 10. List player's wallets");
    Console.WriteLine(" 11. Block/unblock wallet");
    Console.WriteLine(" 12. Exit");
    Console.WriteLine();
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();
    Console.WriteLine();

    if (choice == "12")
    {
        Console.WriteLine("Bye!");
        logger.Info("App shutting down");
        LogManager.Shutdown(); // flushes file writes before exit
        return;
    }

    try
    {
        HandleChoice(choice);
    }
    catch (Exception ex)
    {
        // Safety net: log anything a specific handler didn't catch, keep the app alive.
        logger.Error(ex, "Unexpected error while handling option {Choice}", choice);
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }

    Pause();
}

void HandleChoice(string? choice)
{
    switch (choice)
    {
        case "1": AddPlayer(); break;
        case "2": ListPlayers(); break;
        case "3": FindByName(); break;
        case "4": UpdateScore(); break;
        case "5": DeletePlayer(); break;
        case "6": GroupPlayersByScore(); break;
        case "7": AddWallet(); break;
        case "8": Deposit(); break;
        case "9": Withdraw(); break;
        case "10": ListWallets(); break;
        case "11": ToggleWalletBlock(); break;
        default: Console.WriteLine("Invalid option, try again."); break;
    }
}

// Waits for Enter so the user can read the result before the menu shows again.
void Pause()
{
    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

// Prompts for a player name and resolves it against the repository (case-insensitive)
IPlayer? PromptForPlayer(string label = "Player name: ")
{
    Console.Write(label);
    var name = Console.ReadLine()?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Please type a name.");
        return null;
    }

    var player = playerRepository.GetAll().FirstOrDefault(p =>
        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    if (player is null)
        Console.WriteLine($"No player named \"{name}\".");

    return player;
}

// Prompts for a currency and parses it, or null if invalid.
// The list of valid codes is derived from the enum, so it stays correct
// no matter how many currencies exist.
Currency? PromptForCurrency()
{
    var validCodes = string.Join("/", Enum.GetNames<Currency>());
    Console.Write($"Currency ({validCodes}): ");
    var input = Console.ReadLine()?.Trim() ?? string.Empty;

    // Enum.TryParse also accepts raw numbers, so reject anything not defined.
    if (!Enum.TryParse<Currency>(input, ignoreCase: true, out var currency)
        || !Enum.IsDefined(currency))
    {
        Console.WriteLine($"Unknown currency. Use one of: {validCodes}.");
        return null;
    }

    return currency;
}

// Prompts for a player + currency (both must resolve), or null.
(IPlayer Player, Currency Currency)? PromptForPlayerAndCurrency()
{
    var player = PromptForPlayer();
    if (player is null) return null;

    var currency = PromptForCurrency();
    if (currency is null) return null;

    return (player, currency.Value);
}

// Runs a wallet operation, turning any domain failure into a friendly message
// + a Warn log. The repository already logged the success path.
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

void AddPlayer()
{
    Console.Write("Name: ");
    var name = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Name cannot be empty.");
        return;
    }

    // Block duplicate names so a name uniquely identifies a player
    var alreadyExists = playerRepository.GetAll().Any(p =>
        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    if (alreadyExists)
    {
        Console.WriteLine($"A player named \"{name}\" already exists.");
        return;
    }

    // The constructor generates the Id and starts the score at 0.
    // The repository logs the addition at the service layer.
    var player = new Player(name);
    playerRepository.AddPlayer(player);
    Console.WriteLine($"Added: {player}");
}

void ListPlayers()
{
    var players = playerRepository.GetAll().ToList();

    if (players.Count == 0)
    {
        Console.WriteLine("No players yet.");
        return;
    }

    Console.WriteLine("--- Players ---");
    foreach (var player in players)
        Console.WriteLine(player);
}

void FindByName()
{
    var player = PromptForPlayer("Search name: ");
    if (player is null) return;

    Console.WriteLine($"Found: {player}");
}

void UpdateScore()
{
    var player = PromptForPlayer();
    if (player is null) return;

    Console.Write("Points to add: ");
    if (!int.TryParse(Console.ReadLine(), out var points) || points < 0)
    {
        Console.WriteLine("Points must be a non-negative whole number.");
        return;
    }

    // Score changes only through the method (encapsulation)
    player.AddScore(points);
    Console.WriteLine($"Updated: {player}");
    logger.Info("Score updated for player {PlayerId}: +{Points}", player.Id, points);
}

void DeletePlayer()
{
    var player = PromptForPlayer();
    if (player is null) return;

    // The repository logs the deletion (and cascades the wallets).
    playerRepository.DeletePlayer(player.Id);
    Console.WriteLine($"Deleted: {player.Name}");
}

void GroupPlayersByScore()
{
    // LINQ: GroupBy buckets players that share the same score
    var groups = playerRepository.GroupPlayersByScore()
        .OrderByDescending(g => g.Key)
        .ToList();

    if (groups.Count == 0)
    {
        Console.WriteLine("No players yet.");
        return;
    }

    foreach (var group in groups)
    {
        var names = string.Join(", ", group.Select(p => p.Name));
        Console.WriteLine($"Score {group.Key}: {names}");
    }
}

void AddWallet()
{
    var pc = PromptForPlayerAndCurrency();
    if (pc is null) return;
    var (player, currency) = pc.Value;

    try
    {
        // Defence in depth: don't blindly trust that the resolved player
        // still exists in the repository before we attach a wallet to it.
        if (playerRepository.FindPlayer(player.Id) is null)
            throw new PlayerNotFoundException(player.Id);

        walletRepository.Add(new Wallet(currency), player.Id);
        Console.WriteLine($"Added {currency} wallet to {player.Name}.");
    }
    catch (PlayerNotFoundException ex)
    {
        logger.Warn(ex, "Could not add wallet: player {PlayerId} not found", player.Id);
        Console.WriteLine($"Error: {ex.Message}");
    }
    catch (WalletException ex)
    {
        logger.Warn(ex, "Could not add wallet for player {PlayerId} in {Currency}", player.Id, currency);
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void Deposit()
{
    var pc = PromptForPlayerAndCurrency();
    if (pc is null) return;
    var (player, currency) = pc.Value;

    Console.Write("Amount to deposit: ");
    if (!decimal.TryParse(Console.ReadLine(), out var amount))
    {
        Console.WriteLine("Amount must be a number.");
        return;
    }

    RunWalletOperation(() =>
    {
        walletRepository.Deposit(player.Id, currency, amount);
        Console.WriteLine("Deposit successful.");
    });
}

void Withdraw()
{
    var pc = PromptForPlayerAndCurrency();
    if (pc is null) return;
    var (player, currency) = pc.Value;

    Console.Write("Amount to withdraw: ");
    if (!decimal.TryParse(Console.ReadLine(), out var amount))
    {
        Console.WriteLine("Amount must be a number.");
        return;
    }

    RunWalletOperation(() =>
    {
        walletRepository.Withdraw(player.Id, currency, amount);
        Console.WriteLine("Withdrawal successful.");
    });
}

void ListWallets()
{
    var player = PromptForPlayer();
    if (player is null) return;

    var wallets = walletRepository.GetByPlayer(player.Id).ToList();

    if (wallets.Count == 0)
    {
        Console.WriteLine($"{player.Name} has no wallets.");
        return;
    }

    Console.WriteLine($"--- {player.Name}'s wallets ---");
    foreach (var wallet in wallets)
        Console.WriteLine(wallet);
}

void ToggleWalletBlock()
{
    var pc = PromptForPlayerAndCurrency();
    if (pc is null) return;
    var (player, currency) = pc.Value;

    var wallet = walletRepository.GetByPlayer(player.Id)
        .FirstOrDefault(w => w.Currency == currency);

    if (wallet is null)
    {
        Console.WriteLine($"{player.Name} has no {currency} wallet.");
        return;
    }

    RunWalletOperation(() =>
    {
        if (wallet.IsBlocked)
        {
            walletRepository.Unblock(player.Id, currency);
            Console.WriteLine("Wallet unblocked.");
        }
        else
        {
            walletRepository.Block(player.Id, currency);
            Console.WriteLine("Wallet blocked.");
        }
    });
}

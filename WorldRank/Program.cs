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
    //Console.Clear();
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

    switch (choice)
    {
        case "1":
            AddPlayer();
            break;
        case "2":
            ListPlayers();
            break;
        case "3":
            FindByName();
            break;
        case "4":
            UpdateScore();
            break;
        case "5":
            DeletePlayer();
            break;
        case "6":
            GroupPlayersByScore();
            break;
        case "7":
            AddWallet();
            break;
        case "8":
            Deposit();
            break;
        case "9":
            Withdraw();
            break;
        case "10":
            ListWallets();
            break;
        case "11":
            ToggleWalletBlock();
            break;
        case "12":
            Console.WriteLine("Bye!");
            logger.Info("App shutting down");
            LogManager.Shutdown(); // flushes file writes before exit
            return;
        default:
            Console.WriteLine("Invalid option, try again.");
            break;
    }

    Pause();
}

// Waits for Enter so the user can read the result
// before the screen clears and the menu shows again.
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

// Prompts for a currency and parses it, or null if invalid
Currency? PromptForCurrency()
{
    Console.Write("Currency (EUR/USD): ");
    var input = Console.ReadLine()?.Trim() ?? string.Empty;

    if (!Enum.TryParse<Currency>(input, ignoreCase: true, out var currency))
    {
        Console.WriteLine("Unknown currency. Use EUR or USD.");
        return null;
    }

    return currency;
}

// Resolves a player + currency to one of that player's wallets
IWallet? PromptForWallet()
{
    var player = PromptForPlayer();
    if (player is null) return null;

    var currency = PromptForCurrency();
    if (currency is null) return null;

    var wallet = walletRepository.GetByPlayer(player.Id)
        .FirstOrDefault(w => w.Currency == currency);

    if (wallet is null)
        Console.WriteLine($"{player.Name} has no {currency} wallet.");

    return wallet;
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

    // The constructor generates the Id and starts the score at 0
    var player = new Player(name);
    playerRepository.AddPlayer(player);
    Console.WriteLine($"Added: {player}");
    logger.Info("Player added: {Player}", player);
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
    {
        Console.WriteLine(player);
    }
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

    playerRepository.DeletePlayer(player.Id);
    Console.WriteLine($"Deleted: {player.Name}");
    logger.Info("Player deleted: {PlayerId} ({Name})", player.Id, player.Name);
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
    var player = PromptForPlayer();
    if (player is null) return;

    var currency = PromptForCurrency();
    if (currency is null) return;

    try
    {
        walletRepository.Add(new Wallet(currency.Value), player.Id);
        Console.WriteLine($"Added {currency} wallet to {player.Name}.");
        logger.Info("Wallet added for player {PlayerId} in {Currency}", player.Id, currency);
    }
    catch (DuplicateWalletException ex)
    {
        // Expected & recoverable: the user picked a currency they already have.
        Console.WriteLine(ex.Message);
        logger.Warn("Duplicate wallet rejected for player {PlayerId} in {Currency}",
            ex.PlayerId, ex.Currency);
    }
}

void Deposit()
{
    var wallet = PromptForWallet();
    if (wallet is null) return;

    Console.Write("Amount to deposit: ");
    if (!decimal.TryParse(Console.ReadLine(), out var amount))
    {
        Console.WriteLine("Amount must be a number.");
        return;
    }

    try
    {
        wallet.Deposit(amount);
        Console.WriteLine($"New balance: {wallet}");
        logger.Info("Deposit of {Amount} into {Currency} wallet succeeded", amount, wallet.Currency);
    }
    catch (WalletBlockedException ex)
    {
        // Business rule violated -> custom exception, logged with typed data.
        Console.WriteLine(ex.Message);
        logger.Warn(ex, "Deposit blocked on {Currency} wallet", ex.Currency);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        // Caller misused the API (non-positive amount).
        Console.WriteLine("Deposit amount must be positive.");
        logger.Warn(ex, "Invalid deposit amount {Amount} on {Currency} wallet", amount, wallet.Currency);
    }
}

void Withdraw()
{
    var wallet = PromptForWallet();
    if (wallet is null) return;

    Console.Write("Amount to withdraw: ");
    if (!decimal.TryParse(Console.ReadLine(), out var amount))
    {
        Console.WriteLine("Amount must be a number.");
        return;
    }

    try
    {
        wallet.Withdraw(amount);
        Console.WriteLine($"New balance: {wallet}");
        logger.Info("Withdrawal of {Amount} from {Currency} wallet succeeded", amount, wallet.Currency);
    }
    catch (InsufficientFundsException ex)
    {
        // Most specific first: carries Balance + RequestedAmount as typed data.
        Console.WriteLine(ex.Message);
        logger.Warn("Withdrawal declined: {Requested} requested but only {Balance} in {Currency} wallet",
            ex.RequestedAmount, ex.Balance, ex.Currency);
    }
    catch (WalletBlockedException ex)
    {
        Console.WriteLine(ex.Message);
        logger.Warn(ex, "Withdrawal blocked on {Currency} wallet", ex.Currency);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine("Withdrawal amount must be positive.");
        logger.Warn(ex, "Invalid withdrawal amount {Amount} on {Currency} wallet", amount, wallet.Currency);
    }
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
    {
        Console.WriteLine(wallet);
    }
}

void ToggleWalletBlock()
{
    var wallet = PromptForWallet();
    if (wallet is null) return;

    if (wallet.IsBlocked)
    {
        wallet.Unblock();
        Console.WriteLine($"Unblocked: {wallet}");
    }
    else
    {
        wallet.Block();
        Console.WriteLine($"Blocked: {wallet}");
    }
}

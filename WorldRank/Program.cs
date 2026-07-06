using WorldRank;

// In-memory data store: the list of players lives while the program runs
var players = new List<Player>();

while (true)
{
    Console.Clear();
    Console.WriteLine("========================================");
    Console.WriteLine("      WorldRank — Player Registry       ");
    Console.WriteLine("========================================");
    Console.WriteLine();
    Console.WriteLine("  1. Add player");
    Console.WriteLine("  2. List players");
    Console.WriteLine("  3. Find by name");
    Console.WriteLine("  4. Update score");
    Console.WriteLine("  5. Exit");
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
            Console.WriteLine("Bye!");
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
    var alreadyExists = players.Any(p =>
        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    if (alreadyExists)
    {
        Console.WriteLine($"A player named \"{name}\" already exists.");
        return;
    }

    // The constructor generates the Id and starts the score at 0
    var player = new Player(name);
    players.Add(player);
    Console.WriteLine($"Added: {player}");
}

void ListPlayers()
{
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
    Console.Write("Search name: ");
    var term = Console.ReadLine()?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(term))
    {
        Console.WriteLine("Please type a name to search.");
        return;
    }

    // LINQ: FirstOrDefault returns null if no match is found (case-insensitive)
    var match = players.FirstOrDefault(p =>
        p.Name.Equals(term, StringComparison.OrdinalIgnoreCase));

    if (match is null)
    {
        Console.WriteLine($"No player named \"{term}\".");
        return;
    }

    Console.WriteLine($"Found: {match}");
}

void UpdateScore()
{
    Console.Write("Player name: ");
    var name = Console.ReadLine()?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Please type a name.");
        return;
    }

    var player = players.FirstOrDefault(p =>
        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    if (player is null)
    {
        Console.WriteLine($"No player named \"{name}\".");
        return;
    }

    Console.Write("Points to add: ");
    if (!int.TryParse(Console.ReadLine(), out var points) || points < 0)
    {
        Console.WriteLine("Points must be a non-negative whole number.");
        return;
    }

    // Score changes only through the method (encapsulation)
    player.AddScore(points);
    Console.WriteLine($"Updated: {player}");
}

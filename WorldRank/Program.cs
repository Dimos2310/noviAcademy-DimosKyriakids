using WorldRank;

// In-memory data store: the list of players lives while the program runs
var players = new List<Player>();
var nextId = 1;

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
    Console.WriteLine("  4. Exit");
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

    var player = new Player
    {
        Id = nextId++,
        Name = name.Trim(),
        Score = score
    };

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

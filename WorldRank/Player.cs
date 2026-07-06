namespace WorldRank;

// Player: Id is auto-generated; Score changes only through AddScore
public class Player
{
    public Guid Id { get; }                   // read-only: set once in the constructor
    public string Name { get; set; }
    public int Score { get; private set; }    // read anywhere, written only inside the class

    public Player(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Score = 0;
    }

    // The only way to change the score (keeps Score encapsulated)
    public void AddScore(int points)
    {
        if (points < 0)
            throw new ArgumentOutOfRangeException(nameof(points));

        Score += points;
    }

    public override string ToString() => $"{Name} (Score: {Score})";
}

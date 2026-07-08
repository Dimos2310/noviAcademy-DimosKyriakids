namespace WorldRank;

// Player: Id is auto-generated; Score changes only through AddScore
public class Player : IPlayer
{
    private static int _nextId = 1;

    public int Id { get; }                     // read-only: assigned once in the constructor
    public string Name { get; }                 // read-only: no public setter
    public int Score { get; private set; }      // read anywhere, written only inside the class

    public Player(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = _nextId++;
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

    public override string ToString() => $"#{Id} {Name} (Score: {Score})";
}

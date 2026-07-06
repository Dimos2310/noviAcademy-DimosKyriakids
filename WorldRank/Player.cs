namespace WorldRank;

// Player: Id, Name, Score
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }

    public override string ToString()
    {
        return $"#{Id,-3} {Name,-20} Score: {Score}";
    }
}

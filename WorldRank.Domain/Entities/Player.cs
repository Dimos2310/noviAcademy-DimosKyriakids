namespace WorldRank.Domain.Entities;

public class Player : IPlayer
{
	public int Id { get; }
	public string Name { get; private set; }
	public int Score { get; private set; }

	public Player(int id, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be empty.", nameof(name));

		Id = id;
		Name = name;
		Score = 0;
	}

	public void AddScore(int points)
	{
		if (points < 0)
			throw new ArgumentOutOfRangeException(nameof(points), "Points cannot be negative.");

		Score += points;
	}

	public override string ToString() => $"[{Id}] {Name} - Score: {Score}";
}

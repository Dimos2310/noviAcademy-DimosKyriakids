namespace WorldRank.Console
{
	public interface IPlayer
	{
		int Id { get; }
		string Name { get; }
		int Score { get; }

		void AddScore(int points);
	}
}

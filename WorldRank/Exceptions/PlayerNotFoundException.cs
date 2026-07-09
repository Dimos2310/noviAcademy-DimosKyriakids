namespace WorldRank.Console.Exceptions
{
	public class PlayerNotFoundException : Exception
	{
		public int PlayerId { get; }

		public PlayerNotFoundException(int playerId)
			: base($"Player {playerId} was not found.")
		{
			PlayerId = playerId;
		}
	}
}

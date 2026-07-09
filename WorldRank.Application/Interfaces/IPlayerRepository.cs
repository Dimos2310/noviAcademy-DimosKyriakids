using WorldRank.Domain.Entities;

namespace WorldRank.Application.Interfaces
{
	public interface IPlayerRepository
	{
		void AddPlayer(Player player);

		IEnumerable<Player> GetAllPlayers();

		void DeletePlayer(int playerId);

		Player? FindPlayer(int playerId);

		IEnumerable<IGrouping<int, Player>> GroupPlayersByScore();
	}
}

using WorldRank.Domain.Entities;

namespace WorldRank.Application.Interfaces;

public interface IPlayerService
{
	Player AddPlayer(string name, int score);

	IEnumerable<Player> GetAllPlayers();

	IEnumerable<IGrouping<int, Player>> GroupPlayersByScore();

	Player? FindPlayerByName(string name);

	Player? FindPlayerById(int playerId);

	void DeletePlayer(int playerId);
}

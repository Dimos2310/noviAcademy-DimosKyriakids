using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Services;

// Use-case logic only. No console / presentation concerns: inputs come in as
// parameters, results are returned, failures surface as domain exceptions.
public class PlayerService
{
	private readonly IPlayerRepository _playerRepository;

	public PlayerService(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public Player AddPlayer(string name, int score)
	{
		var player = new Player(GeneratePlayerId(), name);
		player.AddScore(score);
		_playerRepository.AddPlayer(player);
		return player;
	}

	public IEnumerable<Player> GetAllPlayers()
	{
		return _playerRepository.GetAllPlayers();
	}

	public IEnumerable<IGrouping<int, Player>> GroupPlayersByScore()
	{
		return _playerRepository.GroupPlayersByScore();
	}

	public Player? FindPlayerByName(string name)
	{
		return _playerRepository.GetAllPlayers()
			.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	public Player? FindPlayerById(int playerId)
	{
		return _playerRepository.FindPlayer(playerId);
	}

	public void DeletePlayer(int playerId)
	{
		_playerRepository.DeletePlayer(playerId);
	}

	// Generates a random, unique player id (avoids collisions with already-registered players).
	private int GeneratePlayerId()
	{
		var existingIds = _playerRepository.GetAllPlayers().Select(p => p.Id).ToHashSet();

		int id;
		do
		{
			id = Random.Shared.Next(1, int.MaxValue);
		}
		while (existingIds.Contains(id));

		return id;
	}
}

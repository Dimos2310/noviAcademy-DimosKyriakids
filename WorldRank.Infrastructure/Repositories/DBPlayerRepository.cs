using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Infrastructure.Persistence.Context;

namespace WorldRank.Infrastructure.Repositories;

public class DBPlayerRepository : IPlayerRepository
{
	private readonly WorldRankDbContext _context;
	private readonly ILogger<DBPlayerRepository> _logger;

	public DBPlayerRepository(WorldRankDbContext context, ILogger<DBPlayerRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public void AddPlayer(Player player)
	{
		_context.Players.Add(player);
		_context.SaveChanges();
		_logger.LogInformation("Player {PlayerId} ({Name}) added to database with score {Score}", player.Id, player.Name, player.Score);
	}

	public IEnumerable<Player> GetAllPlayers()
	{
		return _context.Players.AsNoTracking().ToList();
	}

	public void DeletePlayer(int playerId)
	{
		var player = _context.Players.FirstOrDefault(item => item.Id == playerId);

		if (player is null)
		{
			_logger.LogWarning("Delete skipped: player {PlayerId} not found in database", playerId);
			return;
		}

		_context.Players.Remove(player);
		_context.SaveChanges();
		_logger.LogInformation("Player {PlayerId} deleted from database", playerId);
	}

	public Player? FindPlayer(int playerId)
	{
		// Read-only existence check, so no need to track the entity.
		return _context.Players.AsNoTracking().FirstOrDefault(item => item.Id == playerId);
	}

	public IEnumerable<IGrouping<int, Player>> GroupPlayersByScore()
	{
		// Fetch from DB, then group in-memory to preserve exact GroupBy alignment & ordering
		return _context.Players
			.AsNoTracking()
			.ToList()
			.GroupBy(player => player.Score)
			.OrderByDescending(group => group.Key);
	}
}

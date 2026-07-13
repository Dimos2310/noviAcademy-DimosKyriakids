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

	public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
	{
		_context.Players.Add(player);
		await _context.SaveChangesAsync(cancellationToken);
		_logger.LogInformation("Player {PlayerId} ({Name}) added to database with score {Score}", player.Id, player.Name, player.Score);
	}

	public async Task<IReadOnlyList<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Players.AsNoTracking().ToListAsync(cancellationToken);
	}

	public async Task DeletePlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		var player = await _context.Players.FirstOrDefaultAsync(item => item.Id == playerId, cancellationToken);

		if (player is null)
		{
			_logger.LogWarning("Delete skipped: player {PlayerId} not found in database", playerId);
			return;
		}

		_context.Players.Remove(player);
		await _context.SaveChangesAsync(cancellationToken);
		_logger.LogInformation("Player {PlayerId} deleted from database", playerId);
	}

	public async Task<Player?> FindPlayerAsync(int playerId, CancellationToken cancellationToken = default)
	{
		// Read-only existence check, so no need to track the entity.
		return await _context.Players.AsNoTracking().FirstOrDefaultAsync(item => item.Id == playerId, cancellationToken);
	}

	public async Task<IEnumerable<IGrouping<int, Player>>> GroupPlayersByScoreAsync(CancellationToken cancellationToken = default)
	{
		// Fetch from DB, then group in-memory to preserve exact GroupBy alignment & ordering
		var players = await _context.Players.AsNoTracking().ToListAsync(cancellationToken);

		return players
			.GroupBy(player => player.Score)
			.OrderByDescending(group => group.Key);
	}
}

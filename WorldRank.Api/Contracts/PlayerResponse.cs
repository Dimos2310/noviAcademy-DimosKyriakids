using WorldRank.Domain.Entities;

namespace WorldRank.Api.Contracts;

// Response DTO — the shape the client receives. Never expose the domain entity directly.
public record PlayerResponse(int Id, string Name, int Score)
{
	public static PlayerResponse From(Player player) => new(player.Id, player.Name, player.Score);
}

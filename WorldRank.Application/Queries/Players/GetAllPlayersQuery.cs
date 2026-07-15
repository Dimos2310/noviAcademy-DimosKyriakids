using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public record GetAllPlayersQuery : IRequest<IReadOnlyList<Player>>;

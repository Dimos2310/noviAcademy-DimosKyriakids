using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public record GetPlayersGroupedByScoreQuery : IRequest<IEnumerable<IGrouping<int, Player>>>;

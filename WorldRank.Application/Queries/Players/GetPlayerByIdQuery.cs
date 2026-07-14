using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public record GetPlayerByIdQuery(int PlayerId) : IRequest<Player?>;

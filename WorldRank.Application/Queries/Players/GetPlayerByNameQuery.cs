using MediatR;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public record GetPlayerByNameQuery(string Name) : IRequest<Player?>;

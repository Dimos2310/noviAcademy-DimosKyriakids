using MediatR;

namespace WorldRank.Application.Commands.Players;

public record AddPlayerCommand(string Name, int Score) : IRequest<int>;

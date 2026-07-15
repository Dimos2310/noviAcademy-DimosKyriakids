using MediatR;

namespace WorldRank.Application.Commands.Players;

public record DeletePlayerCommand(int PlayerId) : IRequest;

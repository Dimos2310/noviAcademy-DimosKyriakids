using MediatR;
using WorldRank.Application.Interfaces;

namespace WorldRank.Application.Commands.Players;

public class DeletePlayerCommandHandler : IRequestHandler<DeletePlayerCommand>
{
	private readonly IPlayerRepository _playerRepository;

	public DeletePlayerCommandHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public async Task Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
	{
		await _playerRepository.DeletePlayerAsync(request.PlayerId, cancellationToken);
	}
}

using MediatR;
using WorldRank.Application.Interfaces;

namespace WorldRank.Application.Commands.Players;

public class DeletePlayerCommandHandler : IRequestHandler<DeletePlayerCommand>
{
	private readonly IPlayerWriteRepository _playerWriteRepository;

	public DeletePlayerCommandHandler(IPlayerWriteRepository playerWriteRepository)
	{
		_playerWriteRepository = playerWriteRepository;
	}

	public async Task Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
	{
		await _playerWriteRepository.DeletePlayerAsync(request.PlayerId, cancellationToken);
	}
}

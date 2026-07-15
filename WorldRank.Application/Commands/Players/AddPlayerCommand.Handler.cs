using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Players;

public class AddPlayerCommandHandler : IRequestHandler<AddPlayerCommand, int>
{
	private readonly IPlayerWriteRepository _playerWriteRepository;

	public AddPlayerCommandHandler(IPlayerWriteRepository playerWriteRepository)
	{
		_playerWriteRepository = playerWriteRepository;
	}

	public async Task<int> Handle(AddPlayerCommand request, CancellationToken cancellationToken)
	{
		var player = new Player(request.Name);
		player.AddScore(request.Score);

		await _playerWriteRepository.AddPlayerAsync(player, cancellationToken);
		return player.Id;
	}
}

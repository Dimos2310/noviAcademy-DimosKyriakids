using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Commands.Players;

public class AddPlayerCommandHandler : IRequestHandler<AddPlayerCommand, int>
{
	private readonly IPlayerRepository _playerRepository;

	public AddPlayerCommandHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public async Task<int> Handle(AddPlayerCommand request, CancellationToken cancellationToken)
	{
		var player = new Player(request.Name);
		player.AddScore(request.Score);

		await _playerRepository.AddPlayerAsync(player, cancellationToken);
		return player.Id;
	}
}

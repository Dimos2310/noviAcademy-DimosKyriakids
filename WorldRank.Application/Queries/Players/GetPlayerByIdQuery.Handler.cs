using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetPlayerByIdQueryHandler : IRequestHandler<GetPlayerByIdQuery, Player?>
{
	private readonly IPlayerRepository _playerRepository;

	public GetPlayerByIdQueryHandler(IPlayerRepository playerRepository)
	{
		_playerRepository = playerRepository;
	}

	public Task<Player?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
		=> _playerRepository.FindPlayerAsync(request.PlayerId, cancellationToken);
}

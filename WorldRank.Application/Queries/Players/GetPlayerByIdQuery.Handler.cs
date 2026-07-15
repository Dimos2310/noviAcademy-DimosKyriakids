using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetPlayerByIdQueryHandler : IRequestHandler<GetPlayerByIdQuery, Player?>
{
	private readonly IPlayerReadRepository _playerReadRepository;

	public GetPlayerByIdQueryHandler(IPlayerReadRepository playerReadRepository)
	{
		_playerReadRepository = playerReadRepository;
	}

	public Task<Player?> Handle(GetPlayerByIdQuery request, CancellationToken cancellationToken)
		=> _playerReadRepository.FindPlayerAsync(request.PlayerId, cancellationToken);
}

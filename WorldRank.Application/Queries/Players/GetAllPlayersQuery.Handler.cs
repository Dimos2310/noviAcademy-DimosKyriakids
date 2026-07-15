using MediatR;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.Application.Queries.Players;

public class GetAllPlayersQueryHandler : IRequestHandler<GetAllPlayersQuery, IReadOnlyList<Player>>
{
	private readonly IPlayerReadRepository _playerReadRepository;

	public GetAllPlayersQueryHandler(IPlayerReadRepository playerReadRepository)
	{
		_playerReadRepository = playerReadRepository;
	}

	public Task<IReadOnlyList<Player>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
		=> _playerReadRepository.GetAllPlayersAsync(cancellationToken);
}

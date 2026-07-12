using Microsoft.Extensions.DependencyInjection;
using WorldRank.Application.Interfaces;
using WorldRank.Infrastructure.Repositories;

namespace WorldRank.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		// In-memory repositories must be Singletons to keep their state.
		services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
		services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();

		return services;
	}
}

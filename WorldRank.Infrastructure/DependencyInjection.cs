using Microsoft.Extensions.DependencyInjection;
using WorldRank.Application.Interfaces;
using WorldRank.Infrastructure.Repositories;

namespace WorldRank.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
		services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();

		return services;
	}
}

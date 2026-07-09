using Microsoft.Extensions.DependencyInjection;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Services;
using WorldRank.Application.Strategies;

namespace WorldRank.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IWalletService, WalletService>();

		services.AddScoped<IFundsStrategy, AddFundsStrategy>();
		services.AddScoped<IFundsStrategy, SubtractFundsStrategy>();
		services.AddScoped<IFundsStrategy, ForceSubtractFundsStrategy>();

		return services;
	}
}

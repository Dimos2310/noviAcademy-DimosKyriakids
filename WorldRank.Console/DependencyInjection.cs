using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using WorldRank.Application;
using WorldRank.Infrastructure;

namespace WorldRank.Console;

public static class DependencyInjection
{
	// Composition root: wires up every layer's services in one place.
	public static IServiceCollection AddWorldRank(this IServiceCollection services)
	{
		// Microsoft.Extensions.Logging with NLog as the provider, so components
		// can receive an ILogger<T> through constructor injection.
		services.AddLogging(builder =>
		{
			builder.ClearProviders();
			builder.SetMinimumLevel(LogLevel.Trace); // NLog rules in nlog.config decide the real thresholds
			builder.AddNLog();
		});

		services.AddApplication();
		services.AddInfrastructure();

		return services;
	}
}

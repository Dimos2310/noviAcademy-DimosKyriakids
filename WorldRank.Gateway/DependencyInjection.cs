using Microsoft.Extensions.DependencyInjection;

namespace WorldRank.Gateway;

public static class DependencyInjection
{
	// Registers every typed HttpClient this project owns. Keeps all external-API wiring
	// (base addresses, DTOs, HTTP logic) out of Domain and Application.
	public static IServiceCollection AddGateway(this IServiceCollection services)
	{
		services.AddHttpClient<IEcbHttpClient, EcbHttpClient>(client =>
		{
			client.BaseAddress = new Uri("https://www.ecb.europa.eu/");
		});

		return services;
	}
}

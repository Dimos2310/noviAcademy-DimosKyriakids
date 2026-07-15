using Autofac;
using WorldRank.Application.Interfaces;
using WorldRank.Infrastructure.Caching;

namespace WorldRank.Infrastructure;

public class InfrastructureModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		// IPlayerRepository / IWalletRepository are registered as scoped services in
		// Program.cs (builder.Services.AddScoped<...>) and merged into this container by
		// AutofacServiceProviderFactory. These decorators wrap whichever implementation is
		// registered there (DB or in-memory) with cache-aside reads and write-through writes.
		builder.RegisterDecorator(typeof(PlayerRepositoryCachingDecorator), typeof(IPlayerRepository));
		builder.RegisterDecorator(typeof(WalletRepositoryCachingDecorator), typeof(IWalletRepository));
	}
}

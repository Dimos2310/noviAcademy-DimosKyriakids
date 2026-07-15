using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldRank.Application.Interfaces;
using WorldRank.Infrastructure.Persistence.Context;
using WorldRank.Infrastructure.Repositories;

namespace WorldRank.Infrastructure;

public static class DependencyInjection
{
	// Single switch: true = SQL database-backed repositories, false = in-memory.
	// The consuming code (services, Program.cs) only ever sees the interfaces,
	// so flipping this is the only change needed to swap implementations.
	public static readonly bool UseDatabase = true;

	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		if (UseDatabase)
		{
			// Database-backed repositories share the DbContext lifetime (Scoped).
			// Each interface resolves its own instance of the same class — both share the
			// scoped DbContext, so there's no split-brain risk within a request.
			services.AddScoped<IPlayerReadRepository, DBPlayerRepository>();
			services.AddScoped<IPlayerWriteRepository, DBPlayerRepository>();
			services.AddScoped<IWalletReadRepository, DBWalletRepository>();
			services.AddScoped<IWalletWriteRepository, DBWalletRepository>();
		}
		else
		{
			// In-memory repositories must be Singletons, and BOTH interfaces must resolve
			// the *same* instance — the list lives in the object itself, so two separate
			// instances would mean writes never show up in reads.
			services.AddSingleton<InMemoryPlayerRepository>();
			services.AddSingleton<IPlayerReadRepository>(sp => sp.GetRequiredService<InMemoryPlayerRepository>());
			services.AddSingleton<IPlayerWriteRepository>(sp => sp.GetRequiredService<InMemoryPlayerRepository>());

			services.AddSingleton<InMemoryWalletRepository>();
			services.AddSingleton<IWalletReadRepository>(sp => sp.GetRequiredService<InMemoryWalletRepository>());
			services.AddSingleton<IWalletWriteRepository>(sp => sp.GetRequiredService<InMemoryWalletRepository>());
		}

		// EF Core over SQL Server. SQL authentication (User Id/Password) — the
		// local SQL container does not support Windows/Integrated auth.
		services.AddDbContext<WorldRankDbContext>(options =>
			options.UseSqlServer(
				"Server=localhost\\MSSQLSERVER01;Database=WorldRank;Integrated Security=True;TrustServerCertificate=true"));

		return services;
	}

	// Creates the WorldRank database and its tables from the model on first run.
	// Only touches the database when running database-backed, so in-memory mode
	// never needs SQL Server to be up.
	public static void InitializeDatabase(this IServiceProvider provider)
	{
		if (!UseDatabase)
			return;

		using var scope = provider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<WorldRankDbContext>();

		// EnsureCreated builds the schema once from the model, which is enough for this
		// exercise. A production setup would use EF migrations (dotnet ef migrations add +
		// context.Database.Migrate()) so the schema can evolve without dropping the database.
		try
		{
			context.Database.EnsureCreated();
			Console.WriteLine("Database EnsureCreated completed.");
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("InitializeDatabase failed: " + ex);
			throw;
		}
	}
}

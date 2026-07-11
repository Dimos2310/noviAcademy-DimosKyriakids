using Microsoft.EntityFrameworkCore;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Persistence.Context;

public partial class WorldRankDbContext : DbContext
{
	public DbSet<Player> Players { get; set; }

	public WorldRankDbContext(DbContextOptions<WorldRankDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Player>(x =>
		{
			x.ToTable("Players");
			x.HasKey(x => x.Id);
			x.Property(y => y.Id).ValueGeneratedNever();
			x.Property(y => y.Name).HasMaxLength(100).IsRequired();
			x.Property(y => y.Score).IsRequired();
		});

		base.OnModelCreating(modelBuilder);
	}
}

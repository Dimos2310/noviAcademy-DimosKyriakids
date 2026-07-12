using Microsoft.EntityFrameworkCore;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Persistence.Context;

public partial class WorldRankDbContext : DbContext
{
	public DbSet<Player> Players { get; set; }
	public DbSet<Wallet> Wallets { get; set; }

	public WorldRankDbContext(DbContextOptions<WorldRankDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Player>(x =>
		{
			x.ToTable("Players");
			x.HasKey(x => x.Id);
			// Let the database generate the primary key (IDENTITY) on insert.
			x.Property(y => y.Id).ValueGeneratedOnAdd();
			x.Property(y => y.Name).HasMaxLength(100).IsRequired();
			x.Property(y => y.Score).IsRequired();
		});

		modelBuilder.Entity<Wallet>(x =>
		{
			x.ToTable("Wallets");
			x.HasKey(x => x.Id);
			// Let the database generate the primary key (IDENTITY) on insert.
			x.Property(y => y.Id).ValueGeneratedOnAdd();
			x.Property(y => y.PlayerId).IsRequired();
			x.Property(y => y.Currency).HasConversion<string>().HasMaxLength(10).IsRequired();
			x.Property(y => y.Balance).HasPrecision(18, 4).IsRequired();
			x.Property(y => y.IsBlocked).IsRequired();
			// A player can hold at most one wallet per currency.
			x.HasIndex(y => new { y.PlayerId, y.Currency }).IsUnique();
		});

		base.OnModelCreating(modelBuilder);
	}
}

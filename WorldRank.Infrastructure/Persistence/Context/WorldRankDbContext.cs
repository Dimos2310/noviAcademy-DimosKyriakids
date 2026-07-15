using Microsoft.EntityFrameworkCore;
using WorldRank.Domain.Entities;

namespace WorldRank.Infrastructure.Persistence.Context;

public partial class WorldRankDbContext : DbContext
{
	public DbSet<Player> Players { get; set; }
	public DbSet<Wallet> Wallets { get; set; }
	public DbSet<CurrencyRates> CurrencyRates { get; set; }

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

			// Optimistic concurrency token, DB-generated on every UPDATE. Kept as a
			// shadow property (not a CLR property on Wallet) so it stays an
			// infrastructure concern. Guards Deposit/Withdraw/ApplyStrategy/
			// UpdateBalance against lost updates from concurrent requests.
			x.Property<byte[]>("RowVersion").IsRowVersion();
		});

		modelBuilder.Entity<CurrencyRates>(x =>
		{
			x.ToTable("CurrencyRates");
			x.HasKey(y => y.Id);
			x.Property(y => y.Id).ValueGeneratedOnAdd();
			x.Property(y => y.Currency).HasMaxLength(3).IsRequired();
			x.Property(y => y.Rate).HasPrecision(18, 6).IsRequired();
			x.Property(y => y.Date).IsRequired();
			// One rate per currency per reference date.
			x.HasIndex(y => new { y.Currency, y.Date }).IsUnique();
		});

		base.OnModelCreating(modelBuilder);
	}
}

using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using System.Data.Common;
using System.Text.Json.Serialization;
using WorldRand;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Services;
using WorldRank.Infrastructure.Persistence.Context;
using WorldRank.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Logging via NLog (same nlog.config layout as the Console app).
builder.Logging.ClearProviders();
builder.Logging.AddNLog("nlog.config");

// One AppDbContext per request (scoped) — the EF Core repositories depend on it.
builder.Services.AddDbContext<WorldRankDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IPlayerRepository, DBPlayerRepository>();
builder.Services.AddScoped<IWalletRepository, DBWalletRepository>();

// Single-instance in-memory cache (Day 6). Redis would replace this behind a load balancer.
//builder.Services.AddMemoryCache();

// The services own the caching (read-through + write-through) and reach the DB via the repositories.
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// Accept/emit enums (e.g. Currency) as their string names, not numbers.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger / OpenAPI — interactive API docs at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Serve the Swagger JSON and UI in Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger")); // root → Swagger UI
}

app.MapControllers();

app.Run();
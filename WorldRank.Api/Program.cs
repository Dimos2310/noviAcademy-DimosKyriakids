using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using System.Data.Common;
using System.Text.Json.Serialization;
using WorldRank.Application;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Infrastructure;
using WorldRank.Infrastructure.Caching;
using WorldRank.Infrastructure.Persistence.Context;
using WorldRank.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Autofac replaces the built-in DI container so RegisterGenericDecorator can wrap every
// MediatR handler (LoggingDecorator) — the built-in container has no decorator support.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new ApplicationModule());
    container.RegisterModule(new InfrastructureModule());
});

// Logging via NLog (same nlog.config layout as the Console app).
builder.Logging.ClearProviders();
builder.Logging.AddNLog("nlog.config");

// One AppDbContext per request (scoped) — the EF Core repositories depend on it.
builder.Services.AddDbContext<WorldRankDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IPlayerRepository, DBPlayerRepository>();
builder.Services.AddScoped<IWalletRepository, DBWalletRepository>();

// Single-instance in-memory cache (Day 6). Redis would replace this behind a load balancer.
// Services depend on ICache (Application-owned port), never on IMemoryCache directly.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICache, MemoryCacheStore>();

// All strategies are registered under the same interface; WalletService resolves them
// as a collection and picks the one whose Operation matches - no factory.
builder.Services.AddSingleton<IFundsStrategy, AddFundsStrategy>();
builder.Services.AddSingleton<IFundsStrategy, SubtractFundsStrategy>();
builder.Services.AddSingleton<IFundsStrategy, ForceSubtractFundsStrategy>();

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
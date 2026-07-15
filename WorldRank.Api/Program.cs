using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using Quartz;
using System.Data.Common;
using System.Text.Json.Serialization;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Services;
using WorldRank.Application.Strategies;
using WorldRank.Gateway;
using WorldRank.Infrastructure.Caching;
using WorldRank.Infrastructure.Jobs;
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
// Services depend on ICache (Application-owned port), never on IMemoryCache directly.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICache, MemoryCacheStore>();

// The services own the caching (cache-aside reads + write-through) and reach the DB via the repositories.
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// All strategies are registered under the same interface; WalletService resolves them
// as a collection and picks the one whose Operation matches - no factory.
builder.Services.AddSingleton<IFundsStrategy, AddFundsStrategy>();
builder.Services.AddSingleton<IFundsStrategy, SubtractFundsStrategy>();
builder.Services.AddSingleton<IFundsStrategy, ForceSubtractFundsStrategy>();

// External API communication (typed HttpClients, DTOs) lives in its own project,
// isolated from Domain/Application.
builder.Services.AddGateway();

// Quartz job: periodically fetches the ECB daily reference rates and stores them.
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(UpdateCurrencyRatesJob));

    q.AddJob<UpdateCurrencyRatesJob>(options => options.WithIdentity(jobKey));
    q.AddTrigger(options => options
        .ForJob(jobKey)
        .WithIdentity($"{nameof(UpdateCurrencyRatesJob)}-trigger")
        .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(1).RepeatForever())
        .StartNow());
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

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
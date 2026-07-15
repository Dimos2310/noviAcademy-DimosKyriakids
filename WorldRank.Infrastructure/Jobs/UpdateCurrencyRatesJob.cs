using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using WorldRank.Domain.Entities;
using WorldRank.Gateway;
using WorldRank.Infrastructure.Persistence.Context;

namespace WorldRank.Infrastructure.Jobs;

// Periodically fetches the latest ECB reference rates and stores them as CurrencyRates rows.
[DisallowConcurrentExecution]
public class UpdateCurrencyRatesJob : IJob
{
	private readonly IEcbHttpClient _ecbHttpClient;
	private readonly WorldRankDbContext _context;
	private readonly ILogger<UpdateCurrencyRatesJob> _logger;

	public UpdateCurrencyRatesJob(IEcbHttpClient ecbHttpClient, WorldRankDbContext context, ILogger<UpdateCurrencyRatesJob> logger)
	{
		_ecbHttpClient = ecbHttpClient;
		_context = context;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		var cancellationToken = context.CancellationToken;

		_logger.LogInformation("Fetching latest ECB currency rates...");
		var response = await _ecbHttpClient.GetLatestRatesAsync(cancellationToken);

		// Skip currencies already stored for this reference date (the job can run more than once a day).
		var existingCurrencies = await _context.CurrencyRates
			.Where(rate => rate.Date == response.ReferenceDate)
			.Select(rate => rate.Currency)
			.ToListAsync(cancellationToken);

		var newRates = response.Rates
			.Where(rate => !existingCurrencies.Contains(rate.Currency))
			.Select(rate => new CurrencyRates(rate.Currency, rate.Rate, response.ReferenceDate))
			.ToList();

		if (newRates.Count == 0)
		{
			_logger.LogInformation("Rates for {Date} are already stored — nothing to do.", response.ReferenceDate);
			return;
		}

		_context.CurrencyRates.AddRange(newRates);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Stored {Count} currency rates for {Date}.", newRates.Count, response.ReferenceDate);
	}
}

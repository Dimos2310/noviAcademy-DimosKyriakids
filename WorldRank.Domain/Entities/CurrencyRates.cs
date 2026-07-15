namespace WorldRank.Domain.Entities;

// A single exchange rate for one currency, as published by the ECB for a given reference date.
public class CurrencyRates
{
	public int Id { get; private set; }
	public string Currency { get; private set; }
	public decimal Rate { get; private set; }
	public DateTime Date { get; private set; }

	public CurrencyRates()
	{
		Currency = string.Empty;
	}

	public CurrencyRates(string currency, decimal rate, DateTime date)
	{
		if (string.IsNullOrWhiteSpace(currency))
			throw new ArgumentException("Currency cannot be empty.", nameof(currency));

		if (rate <= 0)
			throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be positive.");

		Currency = currency;
		Rate = rate;
		Date = date;
	}

	public override string ToString() => $"{Currency} = {Rate} (as of {Date:yyyy-MM-dd})";
}

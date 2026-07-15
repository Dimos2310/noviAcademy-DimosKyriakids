using System.Globalization;
using System.Xml.Linq;
using WorldRank.Gateway.Dtos;

namespace WorldRank.Gateway;

// Typed HttpClient: fetches and parses the ECB daily reference-rates feed.
// https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml
public class EcbHttpClient : IEcbHttpClient
{
	private static readonly XNamespace Ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

	private const string RatesPath = "stats/eurofxref/eurofxref-daily.xml";

	private readonly HttpClient _httpClient;

	public EcbHttpClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<EcbRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default)
	{
		await using var stream = await _httpClient.GetStreamAsync(RatesPath, cancellationToken);
		var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

		// The day's rates live in the <Cube time='...'> element, nested inside an outer <Cube>.
		var dayCube = document.Descendants(Ns + "Cube")
			.First(cube => cube.Attribute("time") is not null);

		var referenceDate = DateTime.Parse(dayCube.Attribute("time")!.Value, CultureInfo.InvariantCulture);

		var rates = dayCube.Elements(Ns + "Cube")
			.Select(cube => new EcbRateDto(
				cube.Attribute("currency")!.Value,
				decimal.Parse(cube.Attribute("rate")!.Value, CultureInfo.InvariantCulture)))
			.ToList();

		return new EcbRatesResponse(referenceDate, rates);
	}
}

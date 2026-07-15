namespace WorldRank.Gateway.Dtos;

// The parsed ECB daily feed: a reference date plus every currency's rate against EUR that day.
public record EcbRatesResponse(DateTime ReferenceDate, IReadOnlyList<EcbRateDto> Rates);

namespace WorldRank.Gateway.Dtos;

// The shape of a single <Cube currency='...' rate='...'/> element from the ECB feed.
public record EcbRateDto(string Currency, decimal Rate);

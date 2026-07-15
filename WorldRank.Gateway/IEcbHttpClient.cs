using WorldRank.Gateway.Dtos;

namespace WorldRank.Gateway;

public interface IEcbHttpClient
{
	Task<EcbRatesResponse> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}

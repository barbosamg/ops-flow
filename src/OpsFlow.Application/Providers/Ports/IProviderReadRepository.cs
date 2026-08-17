
using OpsFlow.Application.Providers.Queries.GetProviderOptions;

namespace OpsFlow.Application.Providers.Ports;

public interface IProviderReadRepository
{
    Task<IReadOnlyList<ProviderOptionDto>> GetActiveAsync(
        CancellationToken cancellationToken);
}
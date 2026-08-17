
using OpsFlow.Web.Models.Providers;

namespace OpsFlow.Web.Clients.Providers;

public interface IProvidersApiClient
{
    Task<IReadOnlyList<ProviderOption>> GetProvidersAsync(
        CancellationToken cancellationToken);
}
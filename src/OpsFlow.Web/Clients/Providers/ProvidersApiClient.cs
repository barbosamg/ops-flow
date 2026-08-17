
using System.Net.Http.Json;
using OpsFlow.Web.Models.Providers;

namespace OpsFlow.Web.Clients.Providers;

public sealed class ProvidersApiClient : IProvidersApiClient
{
    private readonly HttpClient _httpClient;

    public ProvidersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProviderOption>> GetProvidersAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            "api/providers",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var providers = await response.Content
            .ReadFromJsonAsync<ProviderOption[]>(cancellationToken);

        return providers
            ?? throw new InvalidOperationException(
                "The Providers API returned an empty response.");
    }
}
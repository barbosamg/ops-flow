using System.Net.Http.Json;
using OpsFlow.Web.Clients.Common;
using OpsFlow.Web.Models.Customers;

namespace OpsFlow.Web.Clients.Customers;

public sealed class CustomersApiClient(HttpClient httpClient) :
    ICustomersApiClient
{
    public async Task<IReadOnlyList<CustomerOption>> GetCustomersAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            "api/customers",
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<CustomerOption[]>(cancellationToken)
            ?? throw new InvalidOperationException(
                "The Customers API returned an empty response.");
    }
}

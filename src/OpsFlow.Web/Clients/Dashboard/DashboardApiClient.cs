using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using OpsFlow.Web.Clients.Common;
using OpsFlow.Web.Models.Dashboard;

namespace OpsFlow.Web.Clients.Dashboard;

public sealed class DashboardApiClient(HttpClient httpClient) :
    IDashboardApiClient
{
    public async Task<DashboardSummary> GetSummaryAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string?>();

        if (fromUtc.HasValue)
        {
            parameters["fromUtc"] = fromUtc.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        if (toUtc.HasValue)
        {
            parameters["toUtc"] = toUtc.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        var requestUri = QueryHelpers.AddQueryString(
            "api/dashboard/summary",
            parameters);

        using var response = await httpClient.GetAsync(
            requestUri,
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<DashboardSummary>(cancellationToken)
            ?? throw new InvalidOperationException(
                "The Dashboard API returned an empty response.");
    }
}

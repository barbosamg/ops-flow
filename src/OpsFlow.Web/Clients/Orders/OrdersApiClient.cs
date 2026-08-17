using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using OpsFlow.Web.Models.Common;
using OpsFlow.Web.Models.Orders;

namespace OpsFlow.Web.Clients.Orders;

public sealed class OrdersApiClient : IOrdersApiClient
{
    private readonly HttpClient _httpClient;

    public OrdersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResponse<OrderListItem>> GetOrdersAsync(
        OrderSearchRequest request,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildRequestUri(request);

        using var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderApiItem>>(
                cancellationToken);

        if (payload is null)
        {
            throw new InvalidOperationException(
                "The Orders API returned an empty response.");
        }

        var items = payload.Items
            .Select(ToListItem)
            .ToArray();

        return new PagedResponse<OrderListItem>(
            items,
            payload.Page,
            payload.PageSize,
            payload.TotalCount);
    }

    private static string BuildRequestUri(OrderSearchRequest request)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["page"] = request.Page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = request.PageSize.ToString(
                CultureInfo.InvariantCulture)
        };

        AddIfPresent(parameters, "search", request.Search);
        AddIfPresent(parameters, "status", request.Status);
        AddIfPresent(parameters, "customerId", request.CustomerId?.ToString());
        AddIfPresent(parameters, "providerId", request.ProviderId?.ToString());

        AddIfPresent(
            parameters,
            "createdFromUtc",
            request.CreatedFromUtc?.ToString(
                "O",
                CultureInfo.InvariantCulture));

        AddIfPresent(
            parameters,
            "createdToUtc",
            request.CreatedToUtc?.ToString(
                "O",
                CultureInfo.InvariantCulture));

        AddIfPresent(
            parameters,
            "minAmount",
            request.MinAmount?.ToString(CultureInfo.InvariantCulture));

        AddIfPresent(
            parameters,
            "maxAmount",
            request.MaxAmount?.ToString(CultureInfo.InvariantCulture));

        AddIfPresent(parameters, "sort", request.Sort);

        return QueryHelpers.AddQueryString("api/orders", parameters);
    }

    private static void AddIfPresent(
        Dictionary<string, string?> parameters,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters[name] = value;
        }
    }

    private static OrderListItem ToListItem(OrderApiItem item)
    {
        return new OrderListItem(
            item.Id,
            item.Number,
            item.CustomerName,
            item.CustomerEmail,
            item.ProviderName,
            item.Amount,
            item.CreatedAtUtc,
            item.Status);
    }
}

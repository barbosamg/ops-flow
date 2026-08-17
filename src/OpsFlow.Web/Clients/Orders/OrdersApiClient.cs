using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using OpsFlow.Web.Clients.Common;
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

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

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

    public async Task<OrderDetails> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"api/orders/{orderId}",
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await ReadDetailsAsync(response, cancellationToken);
    }

    public async Task<OrderDetails> CreateOrderAsync(
        OrderUpsertRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/orders",
            request.ToCreatePayload(),
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await ReadDetailsAsync(response, cancellationToken);
    }

    public async Task<OrderDetails> UpdateOrderAsync(
        Guid orderId,
        OrderUpsertRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"api/orders/{orderId}",
            request.ToUpdatePayload(),
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await ReadDetailsAsync(response, cancellationToken);
    }

    public async Task<OrderRetryAccepted> RetryOrderAsync(
        Guid orderId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/orders/{orderId}/retry");

        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await ApiResponse.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<OrderRetryAccepted>(cancellationToken)
            ?? throw new InvalidOperationException(
                "The Orders API returned an empty retry response.");
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

    private static async Task<OrderDetails> ReadDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken) =>
        await response.Content.ReadFromJsonAsync<OrderDetails>(
            cancellationToken)
        ?? throw new InvalidOperationException(
            "The Orders API returned an empty order response.");
}

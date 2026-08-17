using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Domain.Orders;

namespace OpsFlow.IntegrationTests.Api;

public sealed class BackendFlowTests
{
    private static readonly Uri ApiBaseAddress = new("http://localhost:5153/");

    [Fact]
    public async Task ApiShouldEnforceValidationAndOptimisticConcurrency()
    {
        if (!ShouldRunEndToEndTests())
        {
            return;
        }

        using var client = new HttpClient { BaseAddress = ApiBaseAddress };
        var createResponse = await client.PostAsJsonAsync(
            "api/orders",
            new
            {
                CustomerId = Guid.Parse(
                    "10000000-0000-0000-0000-000000000001"),
                ProviderId = Guid.Parse(
                    "20000000-0000-0000-0000-000000000001"),
                Amount = 150.75m,
                Notes = "Concurrency integration test"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync(createResponse);
        var orderId = created.GetProperty("id").GetGuid();
        var staleRowVersion = created.GetProperty("rowVersion").GetString();

        var firstUpdate = await client.PutAsJsonAsync(
            $"api/orders/{orderId}",
            new
            {
                CustomerId = Guid.Parse(
                    "10000000-0000-0000-0000-000000000001"),
                ProviderId = Guid.Parse(
                    "20000000-0000-0000-0000-000000000002"),
                Amount = 175.25m,
                Notes = "First update",
                RowVersion = staleRowVersion
            });

        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        var staleUpdate = await client.PutAsJsonAsync(
            $"api/orders/{orderId}",
            new
            {
                CustomerId = Guid.Parse(
                    "10000000-0000-0000-0000-000000000001"),
                ProviderId = Guid.Parse(
                    "20000000-0000-0000-0000-000000000003"),
                Amount = 200m,
                Notes = "Stale update",
                RowVersion = staleRowVersion
            });

        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
        var conflict = await ReadJsonAsync(staleUpdate);
        Assert.True(conflict.TryGetProperty("correlationId", out _));

        var invalidRetry = await client.PostAsync(
            "api/orders/00000000-0000-0000-0000-000000000008/retry",
            null);
        Assert.Equal(HttpStatusCode.BadRequest, invalidRetry.StatusCode);
    }

    [Fact]
    public async Task RetryShouldProcessAndPublishSignalRUpdate()
    {
        if (!ShouldRunEndToEndTests())
        {
            return;
        }

        var orderId = Guid.Parse(
            "00000000-0000-0000-0000-000000000008");
        var updateCompletion = new TaskCompletionSource<OrderUpdatedMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        await using var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(ApiBaseAddress, "hubs/orders"))
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<OrderUpdatedMessage>(
            "OrderUpdated",
            message =>
            {
                if (message.OrderId == orderId)
                {
                    updateCompletion.TrySetResult(message);
                }
            });

        await hubConnection.StartAsync();

        using var client = new HttpClient { BaseAddress = ApiBaseAddress };
        using var retryRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/orders/{orderId}/retry");
        retryRequest.Headers.Add(
            "Idempotency-Key",
            $"e2e-{Guid.NewGuid():N}");

        var retryResponse = await client.SendAsync(retryRequest);
        Assert.Equal(HttpStatusCode.Accepted, retryResponse.StatusCode);

        var update = await updateCompletion.Task.WaitAsync(
            TimeSpan.FromSeconds(15));

        Assert.Equal(orderId, update.OrderId);
        Assert.Equal(OrderStatus.Processing, update.Status);

        var finalState = await WaitForFinalStateAsync(client, orderId);
        Assert.Equal(OrderStatus.Failed.ToString(), finalState.Status);
        Assert.Equal(
            IntegrationAttemptStatus.TimedOut.ToString(),
            finalState.AttemptStatus);
    }

    private static bool ShouldRunEndToEndTests() =>
        string.Equals(
            Environment.GetEnvironmentVariable("OPSFLOW_RUN_E2E"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    private static async Task<JsonElement> ReadJsonAsync(
        HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }

    private static async Task<(string Status, string AttemptStatus)>
        WaitForFinalStateAsync(HttpClient client, Guid orderId)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var response = await client.GetAsync($"api/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            var details = await ReadJsonAsync(response);
            var status = details.GetProperty("status").GetString() ?? string.Empty;
            var latestAttempt = details.GetProperty("integrationAttempts")[0];
            var attemptStatus = latestAttempt.GetProperty("status").GetString()
                ?? string.Empty;

            if (attemptStatus is "Succeeded" or "Failed" or "TimedOut")
            {
                return (status, attemptStatus);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        throw new TimeoutException("The retry did not reach a final state.");
    }
}

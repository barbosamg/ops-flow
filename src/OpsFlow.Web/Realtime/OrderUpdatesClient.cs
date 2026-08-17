using Microsoft.AspNetCore.SignalR.Client;
using OpsFlow.Web.Models.Orders;

namespace OpsFlow.Web.Realtime;

public sealed class OrderUpdatesClient : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly SemaphoreSlim _startLock = new(1, 1);

    public OrderUpdatesClient(IConfiguration configuration)
    {
        var apiBaseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException(
                "API base URL is not configured.");

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(apiBaseUrl), "hubs/orders"))
            .WithAutomaticReconnect(
            [
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ])
            .Build();

        _connection.On<OrderUpdatedEvent>(
            "OrderUpdated",
            NotifyOrderUpdatedAsync);

        _connection.Reconnecting += _ => NotifyStateChangedAsync();
        _connection.Reconnected += _ => NotifyStateChangedAsync();
        _connection.Closed += _ => NotifyStateChangedAsync();
    }

    public event Func<OrderUpdatedEvent, Task>? OrderUpdated;
    public event Func<Task>? StateChanged;

    public string State => _connection.State.ToString();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _startLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                // Uma conexão por circuito atende todas as páginas inscritas.
                await _connection.StartAsync(cancellationToken);
                await NotifyStateChangedAsync();
            }
        }
        finally
        {
            _startLock.Release();
        }
    }

    private async Task NotifyOrderUpdatedAsync(OrderUpdatedEvent message)
    {
        var handlers = OrderUpdated?.GetInvocationList()
            .Cast<Func<OrderUpdatedEvent, Task>>()
            .ToArray()
            ?? [];

        foreach (var handler in handlers)
        {
            await handler(message);
        }
    }

    private async Task NotifyStateChangedAsync()
    {
        var handlers = StateChanged?.GetInvocationList()
            .Cast<Func<Task>>()
            .ToArray()
            ?? [];

        foreach (var handler in handlers)
        {
            await handler();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        _startLock.Dispose();
    }
}

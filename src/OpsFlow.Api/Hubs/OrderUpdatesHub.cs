using Microsoft.AspNetCore.SignalR;

namespace OpsFlow.Api.Hubs;

public sealed class OrderUpdatesHub : Hub
{
    public const string OrdersGroupName = "orders";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            OrdersGroupName,
            Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

    public Task SubscribeToOrder(Guid orderId) =>
        Groups.AddToGroupAsync(
            Context.ConnectionId,
            GroupName(orderId),
            Context.ConnectionAborted);

    public Task UnsubscribeFromOrder(Guid orderId) =>
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GroupName(orderId),
            Context.ConnectionAborted);

    public static string GroupName(Guid orderId) => $"order:{orderId:N}";
}

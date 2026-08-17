using OpsFlow.Domain.Common;
using OpsFlow.Domain.Orders;

namespace OpsFlow.UnitTests.Domain.Orders;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateWithValidDataShouldCreateDraftOrder()
    {
        var order = CreateOrder();

        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Equal(125.50m, order.Amount);
        Assert.Empty(order.StatusHistory);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateWithNonPositiveAmountShouldThrow(decimal amount)
    {
        Assert.Throws<DomainRuleException>(() => Order.Create(
            Guid.NewGuid(),
            "ORD-2026-0001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            null,
            Now));
    }

    [Fact]
    public void CompleteFromDraftShouldThrow()
    {
        var order = CreateOrder();

        Assert.Throws<DomainRuleException>(() =>
            order.Complete("worker", Now));
    }

    [Fact]
    public void ValidLifecycleShouldRecordEveryTransition()
    {
        var order = CreateOrder();

        order.Submit("operator", Now.AddMinutes(1));
        order.StartProcessing("worker", Now.AddMinutes(2));
        order.Complete("worker", Now.AddMinutes(3));

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Collection(
            order.StatusHistory,
            item => Assert.Equal(OrderStatus.Pending, item.NewStatus),
            item => Assert.Equal(OrderStatus.Processing, item.NewStatus),
            item => Assert.Equal(OrderStatus.Completed, item.NewStatus));
    }

    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public void UpdateInTerminalStatusShouldThrow(OrderStatus terminalStatus)
    {
        var order = CreateOrder();

        if (terminalStatus == OrderStatus.Cancelled)
        {
            order.Cancel("cancelled", "operator", Now.AddMinutes(1));
        }
        else
        {
            order.Submit("operator", Now.AddMinutes(1));
            order.StartProcessing("worker", Now.AddMinutes(2));
            order.Complete("worker", Now.AddMinutes(3));
        }

        Assert.Throws<DomainRuleException>(() => order.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            250m,
            null,
            Now.AddMinutes(4)));
    }

    [Fact]
    public void QueueRetryForFailedOrderShouldBeIdempotentByCorrelationId()
    {
        var order = CreateFailedOrder();

        var first = order.QueueRetry("correlation-1", Now.AddMinutes(4));
        var duplicate = order.QueueRetry("correlation-1", Now.AddMinutes(5));

        Assert.Same(first, duplicate);
        Assert.Single(order.IntegrationAttempts);
        Assert.Equal(1, first.AttemptNumber);
    }

    [Fact]
    public void QueueRetryWithActiveAttemptShouldThrow()
    {
        var order = CreateFailedOrder();
        order.QueueRetry("correlation-1", Now.AddMinutes(4));

        Assert.Throws<DomainRuleException>(() =>
            order.QueueRetry("correlation-2", Now.AddMinutes(5)));
    }

    [Fact]
    public void FailedRetryShouldAllowNextAttemptAndIncrementNumber()
    {
        var order = CreateFailedOrder();
        var first = order.QueueRetry("correlation-1", Now.AddMinutes(4));
        order.StartAttempt(first.Id, "worker", Now.AddMinutes(5));
        order.FailAttempt(
            first.Id,
            "PROVIDER_REJECTED",
            "Provider rejected the order.",
            "worker",
            Now.AddMinutes(6));

        var second = order.QueueRetry("correlation-2", Now.AddMinutes(7));

        Assert.Equal(2, second.AttemptNumber);
        Assert.Equal(OrderStatus.Failed, order.Status);
    }

    private static Order CreateOrder() => Order.Create(
        Guid.NewGuid(),
        "ORD-2026-0001",
        Guid.NewGuid(),
        Guid.NewGuid(),
        125.50m,
        "Priority order",
        Now);

    private static Order CreateFailedOrder()
    {
        var order = CreateOrder();
        order.Submit("operator", Now.AddMinutes(1));
        order.StartProcessing("worker", Now.AddMinutes(2));
        order.MarkFailed(
            "Provider rejected the order.",
            "worker",
            Now.AddMinutes(3));
        return order;
    }
}


using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Orders;

namespace OpsFlow.IntegrationTests.Infrastructure.Orders;

public sealed class InMemoryOrderReadRepositoryTests
{
    private readonly InMemoryOrderReadRepository _repository = new();

    [Fact]
    public async Task SearchAsyncWithTextShouldSearchOperationalFields()
    {
        var query = new GetOrdersQuery(
            PageSize: 100,
            Search: "northstar");

        var result = await _repository.SearchAsync(
            query,
            CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.All(
            result.Items,
            order => Assert.Equal("Northstar ERP", order.ProviderName));
    }

    [Fact]
    public async Task SearchAsyncWithCombinedFiltersShouldReturnMatches()
    {
        var providerId = Guid.Parse(
            "20000000-0000-0000-0000-000000000002");

        var query = new GetOrdersQuery(
            PageSize: 100,
            Status: OrderStatus.Completed,
            ProviderId: providerId);

        var result = await _repository.SearchAsync(
            query,
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(
            result.Items,
            order => Assert.Equal(OrderStatus.Completed, order.Status));
        Assert.All(
            result.Items,
            order => Assert.Equal(providerId, order.ProviderId));
    }

    [Fact]
    public async Task SearchAsyncShouldSortBeforeApplyingPagination()
    {
        var query = new GetOrdersQuery(
            Page: 1,
            PageSize: 3,
            Sort: "-amount");

        var result = await _repository.SearchAsync(
            query,
            CancellationToken.None);

        decimal[] expectedAmounts =
        [
            12_999.00m,
            8_750.00m,
            5_600.00m
        ];

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(
            expectedAmounts,
            result.Items.Select(order => order.Amount));
    }

    [Fact]
    public async Task SearchAsyncWithRangesShouldReturnMatches()
    {
        var query = new GetOrdersQuery(
            PageSize: 100,
            CreatedFromUtc: new DateTimeOffset(
                2026, 8, 16, 0, 0, 0, TimeSpan.Zero),
            MinAmount: 1_000m,
            MaxAmount: 9_000m);

        var result = await _repository.SearchAsync(
            query,
            CancellationToken.None);

        Assert.Equal(3, result.TotalCount);

        Assert.All(
            result.Items,
            order => Assert.InRange(order.Amount, 1_000m, 9_000m));
    }

    [Fact]
    public async Task SearchAsyncWithCancellationShouldStopProcessing()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.SearchAsync(
                new GetOrdersQuery(),
                cancellationSource.Token));
    }
}
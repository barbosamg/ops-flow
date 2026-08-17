
using OpsFlow.Application.Orders.Queries.GetOrders;

namespace OpsFlow.UnitTests.Application.Orders;

public sealed class GetOrdersQueryValidatorTests
{
    private readonly GetOrdersQueryValidator _validator = new();

    [Fact]
    public void ValidateWithValidQueryShouldBeValid()
    {
        var query = new GetOrdersQuery(
            Page: 1,
            PageSize: 25,
            Search: "northwind",
            Sort: "-createdAtUtc");

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void ValidateWithInvalidPaginationShouldBeInvalid(
        int page,
        int pageSize)
    {
        var query = new GetOrdersQuery(Page: page, PageSize: pageSize);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateWithSearchLongerThanLimitShouldBeInvalid()
    {
        var query = new GetOrdersQuery(Search: new string('a', 101));

        var result = _validator.Validate(query);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetOrdersQuery.Search));
    }

    [Fact]
    public void ValidateWithUnknownSortFieldShouldBeInvalid()
    {
        var query = new GetOrdersQuery(Sort: "password");

        var result = _validator.Validate(query);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetOrdersQuery.Sort));
    }

    [Fact]
    public void ValidateWithInvertedRangesShouldBeInvalid()
    {
        var query = new GetOrdersQuery(
            CreatedFromUtc: DateTimeOffset.UtcNow,
            CreatedToUtc: DateTimeOffset.UtcNow.AddDays(-1),
            MinAmount: 500m,
            MaxAmount: 100m);

        var result = _validator.Validate(query);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetOrdersQuery.CreatedToUtc));

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetOrdersQuery.MaxAmount));
    }
}

namespace OpsFlow.Web.Clients.Orders;

public sealed record OrderSearchRequest(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    string? Status = null,
    Guid? CustomerId = null,
    Guid? ProviderId = null,
    DateTimeOffset? CreatedFromUtc = null,
    DateTimeOffset? CreatedToUtc = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? Sort = null);
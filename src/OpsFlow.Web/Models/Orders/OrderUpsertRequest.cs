namespace OpsFlow.Web.Models.Orders;

public sealed record OrderUpsertRequest(
    Guid CustomerId,
    Guid ProviderId,
    decimal Amount,
    string? Notes,
    string? RowVersion)
{
    public object ToCreatePayload() => new
    {
        CustomerId,
        ProviderId,
        Amount,
        Notes
    };

    public object ToUpdatePayload() => new
    {
        CustomerId,
        ProviderId,
        Amount,
        Notes,
        RowVersion = RowVersion
            ?? throw new InvalidOperationException(
                "RowVersion is required when updating an order.")
    };
}

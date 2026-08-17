namespace OpsFlow.Application.Customers.Queries.GetCustomerOptions;

public sealed record CustomerOptionDto(
    Guid Id,
    string Name,
    string Email);

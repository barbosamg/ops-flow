using OpsFlow.Application.Customers.Queries.GetCustomerOptions;

namespace OpsFlow.Application.Customers.Ports;

public interface ICustomerReadRepository
{
    Task<IReadOnlyList<CustomerOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken);
}

using OpsFlow.Web.Models.Customers;

namespace OpsFlow.Web.Clients.Customers;

public interface ICustomersApiClient
{
    Task<IReadOnlyList<CustomerOption>> GetCustomersAsync(
        CancellationToken cancellationToken);
}

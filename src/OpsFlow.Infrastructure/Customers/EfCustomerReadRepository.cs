using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Customers.Ports;
using OpsFlow.Application.Customers.Queries.GetCustomerOptions;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Customers;

public sealed class EfCustomerReadRepository(OpsFlowDbContext dbContext) :
    ICustomerReadRepository
{
    public async Task<IReadOnlyList<CustomerOptionDto>> GetOptionsAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.IsActive)
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerOptionDto(
                customer.Id,
                customer.Name,
                customer.Email))
            .ToArrayAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Common.Exceptions;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Orders;

public sealed class EfOrderRepository(OpsFlowDbContext dbContext) :
    IOrderRepository
{
    public Task<Order?> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Orders
            .AsSplitQuery()
            .Include(order => order.StatusHistory)
            .Include(order => order.IntegrationAttempts)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<bool> IsActiveCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(
            customer => customer.Id == customerId && customer.IsActive,
            cancellationToken);

    public Task<bool> IsActiveProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken) =>
        dbContext.Providers.AnyAsync(
            provider => provider.Id == providerId && provider.IsActive,
            cancellationToken);

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task SaveChangesAsync(
        Order order,
        byte[]? expectedRowVersion,
        CancellationToken cancellationToken)
    {
        AddNewAggregateChildren(order);

        if (expectedRowVersion is not null)
        {
            dbContext.Entry(order)
                .Property(entity => entity.RowVersion)
                .OriginalValue = expectedRowVersion;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(
                "The order was changed by another operation. Reload it and try again.",
                exception);
        }
        catch (DbUpdateException exception)
        {
            throw new ConflictException(
                "The operation conflicts with the current persisted state.",
                exception);
        }
    }

    private void AddNewAggregateChildren(Order order)
    {
        foreach (var history in order.StatusHistory)
        {
            var entry = dbContext.Entry(history);

            if (entry.State is EntityState.Detached or EntityState.Modified)
            {
                entry.State = EntityState.Added;
            }
        }

        foreach (var attempt in order.IntegrationAttempts)
        {
            var entry = dbContext.Entry(attempt);

            if (entry.State == EntityState.Detached ||
                entry.State == EntityState.Modified &&
                attempt.Status == IntegrationAttemptStatus.Queued)
            {
                entry.State = EntityState.Added;
            }
        }
    }
}

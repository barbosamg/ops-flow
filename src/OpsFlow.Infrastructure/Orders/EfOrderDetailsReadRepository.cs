using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrderDetails;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Orders;

public sealed class EfOrderDetailsReadRepository(OpsFlowDbContext dbContext) :
    IOrderDetailsReadRepository
{
    public async Task<OrderDetailsDto?> GetDetailsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.StatusHistory)
            .Include(entity => entity.IntegrationAttempts)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(entity => entity.Id == order.CustomerId)
            .Select(entity => new { entity.Name, entity.Email })
            .SingleAsync(cancellationToken);

        var providerName = await dbContext.Providers
            .AsNoTracking()
            .Where(entity => entity.Id == order.ProviderId)
            .Select(entity => entity.Name)
            .SingleAsync(cancellationToken);

        return new OrderDetailsDto(
            order.Id,
            order.Number,
            order.CustomerId,
            customer.Name,
            customer.Email,
            order.ProviderId,
            providerName,
            order.Amount,
            order.Status,
            order.Notes,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            Convert.ToBase64String(order.RowVersion),
            order.StatusHistory
                .OrderByDescending(history => history.ChangedAtUtc)
                .Select(history => new OrderStatusHistoryDto(
                    history.Id,
                    history.PreviousStatus,
                    history.NewStatus,
                    history.Reason,
                    history.ChangedBy,
                    history.ChangedAtUtc))
                .ToArray(),
            order.IntegrationAttempts
                .OrderByDescending(attempt => attempt.AttemptNumber)
                .Select(attempt => new IntegrationAttemptDto(
                    attempt.Id,
                    attempt.AttemptNumber,
                    attempt.CorrelationId,
                    attempt.Status,
                    attempt.QueuedAtUtc,
                    attempt.StartedAtUtc,
                    attempt.FinishedAtUtc,
                    attempt.DurationMilliseconds,
                    attempt.ErrorCode,
                    attempt.SanitizedError))
                .ToArray());
    }
}

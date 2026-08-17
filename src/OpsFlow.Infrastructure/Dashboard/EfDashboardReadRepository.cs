using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Dashboard.Ports;
using OpsFlow.Application.Dashboard.Queries.GetDashboardSummary;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Dashboard;

public sealed class EfDashboardReadRepository(OpsFlowDbContext dbContext) :
    IDashboardReadRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var orders = dbContext.Orders.AsNoTracking();

        if (fromUtc.HasValue)
        {
            orders = orders.Where(order => order.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            orders = orders.Where(order => order.CreatedAtUtc <= toUtc.Value);
        }

        var statusCounts = await orders
            .GroupBy(order => order.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToArrayAsync(cancellationToken);

        var providerRows = await (
            from order in orders
            join provider in dbContext.Providers.AsNoTracking()
                on order.ProviderId equals provider.Id
            group order by new { provider.Id, provider.Name }
            into providerGroup
            select new
            {
                ProviderId = providerGroup.Key.Id,
                ProviderName = providerGroup.Key.Name,
                TotalOrders = providerGroup.Count(),
                CompletedOrders = providerGroup.Count(
                    order => order.Status == OrderStatus.Completed)
            })
            .OrderByDescending(row => row.TotalOrders)
            .ToArrayAsync(cancellationToken);

        var volumeRows = await orders
            .GroupBy(order => order.CreatedAtUtc.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .OrderBy(row => row.Date)
            .ToArrayAsync(cancellationToken);

        var total = statusCounts.Sum(item => item.Count);
        var completed = statusCounts
            .Where(item => item.Status == OrderStatus.Completed)
            .Sum(item => item.Count);

        return new DashboardSummaryDto(
            total,
            statusCounts.Where(item => item.Status == OrderStatus.Processing)
                .Sum(item => item.Count),
            statusCounts.Where(item => item.Status == OrderStatus.Failed)
                .Sum(item => item.Count),
            completed,
            CalculateRate(completed, total),
            statusCounts
                .OrderBy(item => item.Status)
                .Select(item => new DashboardStatusCountDto(
                    item.Status.ToString(),
                    item.Count))
                .ToArray(),
            providerRows
                .Select(row => new DashboardProviderPerformanceDto(
                    row.ProviderId,
                    row.ProviderName,
                    row.TotalOrders,
                    row.CompletedOrders,
                    CalculateRate(row.CompletedOrders, row.TotalOrders)))
                .ToArray(),
            volumeRows
                .Select(row => new DashboardVolumePointDto(
                    DateOnly.FromDateTime(row.Date),
                    row.Count))
                .ToArray());
    }

    private static decimal CalculateRate(int completed, int total) =>
        total is 0 ? 0 : Math.Round(completed * 100m / total, 2);
}

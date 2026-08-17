using OpsFlow.Application.Dashboard.Queries.GetDashboardSummary;

namespace OpsFlow.Application.Dashboard.Ports;

public interface IDashboardReadRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken);
}

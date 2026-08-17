using OpsFlow.Web.Models.Dashboard;

namespace OpsFlow.Web.Clients.Dashboard;

public interface IDashboardApiClient
{
    Task<DashboardSummary> GetSummaryAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken);
}

namespace OpsFlow.Web.Models.Dashboard;

public sealed record DashboardSummary(
    int TotalOrders,
    int ProcessingOrders,
    int FailedOrders,
    int CompletedOrders,
    decimal SuccessRate,
    IReadOnlyList<DashboardStatusCount> OrdersByStatus,
    IReadOnlyList<DashboardProviderPerformance> ProviderPerformance,
    IReadOnlyList<DashboardVolumePoint> Volume);

public sealed record DashboardStatusCount(string Status, int Count);

public sealed record DashboardProviderPerformance(
    Guid ProviderId,
    string ProviderName,
    int TotalOrders,
    int CompletedOrders,
    decimal SuccessRate);

public sealed record DashboardVolumePoint(DateOnly Date, int Count);

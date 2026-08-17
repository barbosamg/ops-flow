namespace OpsFlow.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record DashboardSummaryDto(
    int TotalOrders,
    int ProcessingOrders,
    int FailedOrders,
    int CompletedOrders,
    decimal SuccessRate,
    IReadOnlyList<DashboardStatusCountDto> OrdersByStatus,
    IReadOnlyList<DashboardProviderPerformanceDto> ProviderPerformance,
    IReadOnlyList<DashboardVolumePointDto> Volume);

public sealed record DashboardStatusCountDto(string Status, int Count);

public sealed record DashboardProviderPerformanceDto(
    Guid ProviderId,
    string ProviderName,
    int TotalOrders,
    int CompletedOrders,
    decimal SuccessRate);

public sealed record DashboardVolumePointDto(DateOnly Date, int Count);

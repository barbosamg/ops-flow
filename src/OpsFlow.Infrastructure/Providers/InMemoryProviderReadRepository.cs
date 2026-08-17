
using OpsFlow.Application.Providers.Ports;
using OpsFlow.Application.Providers.Queries.GetProviderOptions;

namespace OpsFlow.Infrastructure.Providers;

public sealed class InMemoryProviderReadRepository
    : IProviderReadRepository
{
    private static readonly IReadOnlyList<ProviderOptionDto> Providers =
    [
        new(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "Atlas Gateway",
            "ATLAS"),

        new(
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "Northstar ERP",
            "NORTHSTAR"),

        new(
            Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "Vertex Logistics",
            "VERTEX")
    ];

    public Task<IReadOnlyList<ProviderOptionDto>> GetActiveAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(Providers);
    }
}
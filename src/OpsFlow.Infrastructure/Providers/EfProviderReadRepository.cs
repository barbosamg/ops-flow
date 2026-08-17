using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Providers.Ports;
using OpsFlow.Application.Providers.Queries.GetProviderOptions;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Providers;

public sealed class EfProviderReadRepository(OpsFlowDbContext dbContext) :
    IProviderReadRepository
{
    public async Task<IReadOnlyList<ProviderOptionDto>> GetActiveAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Providers
            .AsNoTracking()
            .Where(provider => provider.IsActive)
            .OrderBy(provider => provider.Name)
            .Select(provider => new ProviderOptionDto(
                provider.Id,
                provider.Name,
                provider.Code))
            .ToArrayAsync(cancellationToken);
}

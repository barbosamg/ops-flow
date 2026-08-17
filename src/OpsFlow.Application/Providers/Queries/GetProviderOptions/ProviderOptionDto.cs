
namespace OpsFlow.Application.Providers.Queries.GetProviderOptions;

public sealed record ProviderOptionDto(
    Guid Id,
    string Name,
    string Code);
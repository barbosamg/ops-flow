using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Providers;

public sealed class Provider
{
    private Provider()
    {
    }

    private Provider(
        Guid id,
        string code,
        string name,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Code = code;
        Name = name;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Provider Create(
        Guid id,
        string code,
        string name,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Provider id is required.");
        }

        return new Provider(
            id,
            RequiredText(code, 40, "Provider code").ToUpperInvariant(),
            RequiredText(name, 160, "Provider name"),
            createdAtUtc);
    }

    public void Deactivate() => IsActive = false;

    private static string RequiredText(
        string value,
        int maximumLength,
        string fieldName)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length is 0 || normalized.Length > maximumLength)
        {
            throw new DomainRuleException(
                $"{fieldName} must contain between 1 and {maximumLength} characters.");
        }

        return normalized;
    }
}

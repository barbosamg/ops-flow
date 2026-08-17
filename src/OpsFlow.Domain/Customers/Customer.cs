using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

    private Customer(
        Guid id,
        string name,
        string email,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = name;
        Email = email;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Customer Create(
        Guid id,
        string name,
        string email,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Customer id is required.");
        }

        return new Customer(
            id,
            RequiredText(name, 160, "Customer name"),
            RequiredText(email, 254, "Customer email"),
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

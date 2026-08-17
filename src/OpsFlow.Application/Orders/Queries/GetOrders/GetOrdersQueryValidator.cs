
using FluentValidation;

namespace OpsFlow.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    private static readonly HashSet<string> AllowedSortValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "number",
            "-number",
            "customerName",
            "-customerName",
            "providerName",
            "-providerName",
            "amount",
            "-amount",
            "createdAtUtc",
            "-createdAtUtc",
            "status",
            "-status"
        };

    public GetOrdersQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(100);

        RuleFor(query => query.Sort)
            .Must(BeAnAllowedSortValue)
            .WithMessage("Sort must contain an allowed field and direction.");

        RuleFor(query => query)
            .Must(HaveValidDateRange)
            .WithName(nameof(GetOrdersQuery.CreatedToUtc))
            .WithMessage("CreatedToUtc must be greater than or equal to CreatedFromUtc.");

        RuleFor(query => query)
            .Must(HaveValidAmountRange)
            .WithName(nameof(GetOrdersQuery.MaxAmount))
            .WithMessage("MaxAmount must be greater than or equal to MinAmount.");
    }

    private static bool BeAnAllowedSortValue(string? sort)
    {
        return string.IsNullOrWhiteSpace(sort) ||
               AllowedSortValues.Contains(sort);
    }

    private static bool HaveValidDateRange(GetOrdersQuery query)
    {
        return !query.CreatedFromUtc.HasValue ||
               !query.CreatedToUtc.HasValue ||
               query.CreatedFromUtc <= query.CreatedToUtc;
    }

    private static bool HaveValidAmountRange(GetOrdersQuery query)
    {
        return !query.MinAmount.HasValue ||
               !query.MaxAmount.HasValue ||
               query.MinAmount <= query.MaxAmount;
    }
}
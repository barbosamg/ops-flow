
using OpsFlow.Web.Models.Orders;
using Telerik.DataSource;

namespace OpsFlow.Web.Clients.Orders;

public static class TelerikOrderRequestMapper
{
    private static readonly Dictionary<string, string> SortFieldMap =
        new(StringComparer.Ordinal)
        {
            [nameof(OrderListItem.Number)] = "number",
            [nameof(OrderListItem.CustomerName)] = "customerName",
            [nameof(OrderListItem.ProviderName)] = "providerName",
            [nameof(OrderListItem.Total)] = "amount",
            [nameof(OrderListItem.CreatedAt)] = "createdAtUtc",
            [nameof(OrderListItem.Status)] = "status"
        };

    public static OrderSearchRequest Map(DataSourceRequest request)
    {
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 5;

        return new OrderSearchRequest(
            Page: page,
            PageSize: pageSize,
            Search: FindSearchValue(request.Filters),
            Sort: MapSort(request.Sorts.FirstOrDefault()));
    }

    private static string? FindSearchValue(
        IEnumerable<IFilterDescriptor> filters)
    {
        foreach (var filter in filters)
        {
            if (filter is FilterDescriptor descriptor &&
                descriptor.Operator == FilterOperator.Contains &&
                descriptor.Value is string value &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (filter is CompositeFilterDescriptor composite)
            {
                var nestedValue = FindSearchValue(
                    composite.FilterDescriptors);

                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }
        }

        return null;
    }

    private static string? MapSort(SortDescriptor? descriptor)
    {
        if (descriptor is null ||
            !SortFieldMap.TryGetValue(descriptor.Member, out var apiField))
        {
            return null;
        }

        var prefix =
            descriptor.SortDirection == ListSortDirection.Descending
                ? "-"
                : string.Empty;

        return $"{prefix}{apiField}";
    }
}
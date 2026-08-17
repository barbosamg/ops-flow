namespace OpsFlow.Web.Models.Orders;

public static class OrderDemoData
{
    public static IReadOnlyList<OrderListItem> All { get; } =
    [
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "ORD-2026-0001",
            "Northwind Market",
            "Atlas Gateway",
            1_240.50m,
            new DateTimeOffset(2026, 8, 17, 9, 15, 0, TimeSpan.FromHours(-3)),
            "Processing"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            "ORD-2026-0002",
            "Contoso Retail",
            "Northstar ERP",
            489.90m,
            new DateTimeOffset(2026, 8, 17, 8, 40, 0, TimeSpan.FromHours(-3)),
            "Completed"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "ORD-2026-0003",
            "Fabrikam Stores",
            "Vertex Logistics",
            8_750.00m,
            new DateTimeOffset(2026, 8, 16, 17, 30, 0, TimeSpan.FromHours(-3)),
            "Failed"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000004"),
            "ORD-2026-0004",
            "Adventure Works",
            "Atlas Gateway",
            219.99m,
            new DateTimeOffset(2026, 8, 16, 14, 10, 0, TimeSpan.FromHours(-3)),
            "Pending"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000005"),
            "ORD-2026-0005",
            "Tailspin Toys",
            "Northstar ERP",
            3_420.75m,
            new DateTimeOffset(2026, 8, 16, 11, 25, 0, TimeSpan.FromHours(-3)),
            "Completed"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000006"),
            "ORD-2026-0006",
            "Wide World Importers",
            "Vertex Logistics",
            12_999.00m,
            new DateTimeOffset(2026, 8, 15, 16, 45, 0, TimeSpan.FromHours(-3)),
            "Processing"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000007"),
            "ORD-2026-0007",
            "Blue Yonder Airlines",
            "Atlas Gateway",
            799.50m,
            new DateTimeOffset(2026, 8, 15, 13, 20, 0, TimeSpan.FromHours(-3)),
            "Cancelled"),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000008"),
            "ORD-2026-0008",
            "Proseware Services",
            "Northstar ERP",
            5_600.00m,
            new DateTimeOffset(2026, 8, 15, 10, 5, 0, TimeSpan.FromHours(-3)),
            "Failed")
    ];
}
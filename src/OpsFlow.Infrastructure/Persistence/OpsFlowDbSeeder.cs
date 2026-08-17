using Microsoft.EntityFrameworkCore;
using OpsFlow.Domain.Customers;
using OpsFlow.Domain.Orders;
using OpsFlow.Domain.Providers;

namespace OpsFlow.Infrastructure.Persistence;

public sealed class OpsFlowDbSeeder(OpsFlowDbContext dbContext)
{
    private static readonly DateTimeOffset SeedTime =
        new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        var customers = CreateCustomers();
        var providers = CreateProviders();

        await dbContext.Customers.AddRangeAsync(customers, cancellationToken);
        await dbContext.Providers.AddRangeAsync(providers, cancellationToken);
        await dbContext.Orders.AddRangeAsync(
            CreateOrders(customers, providers),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Customer[] CreateCustomers() =>
    [
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "Northwind Market",
            "operations@northwind.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            "Contoso Retail",
            "orders@contoso.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000003"),
            "Fabrikam Stores",
            "support@fabrikam.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000004"),
            "Adventure Works",
            "sales@adventure-works.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000005"),
            "Tailspin Toys",
            "orders@tailspin.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000006"),
            "Wide World Importers",
            "operations@wideworld.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000007"),
            "Blue Yonder Airlines",
            "procurement@blueyonder.example",
            SeedTime),
        Customer.Create(
            Guid.Parse("10000000-0000-0000-0000-000000000008"),
            "Proseware Services",
            "billing@proseware.example",
            SeedTime)
    ];

    private static Provider[] CreateProviders() =>
    [
        Provider.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "ATLAS",
            "Atlas Gateway",
            SeedTime),
        Provider.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "NORTHSTAR",
            "Northstar ERP",
            SeedTime),
        Provider.Create(
            Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "VERTEX",
            "Vertex Logistics",
            SeedTime)
    ];

    private static Order[] CreateOrders(
        Customer[] customers,
        Provider[] providers)
    {
        var orders = new[]
        {
            CreateOrder(1, customers[0], providers[0], 1_240.50m, 51),
            CreateOrder(2, customers[1], providers[1], 489.90m, 47),
            CreateOrder(3, customers[2], providers[2], 8_750.00m, 33),
            CreateOrder(4, customers[3], providers[0], 219.99m, 29),
            CreateOrder(5, customers[4], providers[1], 3_420.75m, 26),
            CreateOrder(6, customers[5], providers[2], 12_999.00m, 20),
            CreateOrder(7, customers[6], providers[0], 799.50m, 16),
            CreateOrder(8, customers[7], providers[1], 5_600.00m, 13)
        };

        MoveToProcessing(orders[0], 52);
        MoveToCompleted(orders[1], 48);
        MoveToFailedWithAttempt(orders[2], 34, "PROVIDER_REJECTED");
        orders[3].Submit("seed", SeedTime.AddHours(30));
        MoveToCompleted(orders[4], 27);
        MoveToProcessing(orders[5], 21);
        orders[6].Cancel(
            "Cancelled by the customer.",
            "seed",
            SeedTime.AddHours(17));
        MoveToFailedWithAttempt(orders[7], 14, "PROVIDER_TIMEOUT");

        return orders;
    }

    private static Order CreateOrder(
        int number,
        Customer customer,
        Provider provider,
        decimal amount,
        int createdAfterHours) =>
        Order.Create(
            Guid.Parse($"00000000-0000-0000-0000-{number:D12}"),
            $"ORD-2026-{number:D4}",
            customer.Id,
            provider.Id,
            amount,
            number % 2 is 0 ? null : "Seeded operational order.",
            SeedTime.AddHours(createdAfterHours));

    private static void MoveToProcessing(Order order, int hour)
    {
        order.Submit("seed", SeedTime.AddHours(hour));
        order.StartProcessing("seed-worker", SeedTime.AddHours(hour).AddMinutes(1));
    }

    private static void MoveToCompleted(Order order, int hour)
    {
        MoveToProcessing(order, hour);
        order.Complete("seed-worker", SeedTime.AddHours(hour).AddMinutes(2));
    }

    private static void MoveToFailedWithAttempt(
        Order order,
        int hour,
        string errorCode)
    {
        MoveToProcessing(order, hour);
        order.MarkFailed(
            "Initial provider processing failed.",
            "seed-worker",
            SeedTime.AddHours(hour).AddMinutes(2));

        var attempt = order.QueueRetry(
            $"seed-{order.Id:N}",
            SeedTime.AddHours(hour).AddMinutes(3));
        order.StartAttempt(
            attempt.Id,
            "seed-worker",
            SeedTime.AddHours(hour).AddMinutes(4));

        if (errorCode == "PROVIDER_TIMEOUT")
        {
            order.TimeOutAttempt(
                attempt.Id,
                "seed-worker",
                SeedTime.AddHours(hour).AddMinutes(5));
            return;
        }

        order.FailAttempt(
            attempt.Id,
            errorCode,
            "The provider rejected the order.",
            "seed-worker",
            SeedTime.AddHours(hour).AddMinutes(5));
    }
}

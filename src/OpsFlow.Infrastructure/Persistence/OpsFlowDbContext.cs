using Microsoft.EntityFrameworkCore;
using OpsFlow.Domain.Customers;
using OpsFlow.Domain.Orders;
using OpsFlow.Domain.Providers;
using OpsFlow.Infrastructure.Persistence.Outbox;

namespace OpsFlow.Infrastructure.Persistence;

public sealed class OpsFlowDbContext(DbContextOptions<OpsFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderStatusHistory> OrderStatusHistory =>
        Set<OrderStatusHistory>();

    public DbSet<IntegrationAttempt> IntegrationAttempts =>
        Set<IntegrationAttempt>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpsFlowDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Customers;
using OpsFlow.Domain.Orders;
using OpsFlow.Domain.Providers;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(
            "Orders",
            table => table.HasCheckConstraint(
                "CK_Orders_Amount_Positive",
                "[Amount] > 0"));

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Number).HasMaxLength(40).IsRequired();
        builder.Property(order => order.Amount).HasPrecision(18, 2);
        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(order => order.Notes).HasMaxLength(1_000);
        builder.Property(order => order.RowVersion).IsRowVersion();

        builder.HasIndex(order => order.Number).IsUnique();
        builder.HasIndex(order => new { order.Status, order.CreatedAtUtc });
        builder.HasIndex(order => new { order.ProviderId, order.Status });
        builder.HasIndex(order => new { order.CustomerId, order.CreatedAtUtc });

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Provider>()
            .WithMany()
            .HasForeignKey(order => order.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.StatusHistory)
            .WithOne()
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.IntegrationAttempts)
            .WithOne()
            .HasForeignKey(attempt => attempt.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(order => order.IntegrationAttempts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

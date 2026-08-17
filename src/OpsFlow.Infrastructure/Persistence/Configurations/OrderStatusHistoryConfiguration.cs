using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class OrderStatusHistoryConfiguration :
    IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(history => history.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(history => history.Reason).HasMaxLength(500).IsRequired();
        builder.Property(history => history.ChangedBy).HasMaxLength(160).IsRequired();
        builder.HasIndex(history => new { history.OrderId, history.ChangedAtUtc });
    }
}

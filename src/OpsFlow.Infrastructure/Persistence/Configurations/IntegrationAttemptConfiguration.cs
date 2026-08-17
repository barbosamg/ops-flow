using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class IntegrationAttemptConfiguration :
    IEntityTypeConfiguration<IntegrationAttempt>
{
    public void Configure(EntityTypeBuilder<IntegrationAttempt> builder)
    {
        builder.ToTable("IntegrationAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.CorrelationId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(attempt => attempt.Status)
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(attempt => attempt.ErrorCode).HasMaxLength(80);
        builder.Property(attempt => attempt.SanitizedError).HasMaxLength(500);

        builder.HasIndex(attempt => new
        {
            attempt.OrderId,
            attempt.AttemptNumber
        }).IsUnique();

        builder.HasIndex(attempt => new
        {
            attempt.OrderId,
            attempt.CorrelationId
        }).IsUnique();

        builder.HasIndex(attempt => attempt.OrderId)
            .IsUnique()
            .HasFilter("[Status] IN ('Queued', 'Processing')");
    }
}

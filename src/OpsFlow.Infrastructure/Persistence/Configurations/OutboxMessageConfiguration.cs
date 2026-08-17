using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Infrastructure.Persistence.Outbox;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration :
    IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).HasMaxLength(160).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(500);
        builder.HasIndex(message => new
        {
            message.ProcessedAtUtc,
            message.OccurredAtUtc
        });
    }
}

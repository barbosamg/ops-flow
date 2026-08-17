using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Providers;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");
        builder.HasKey(provider => provider.Id);
        builder.Property(provider => provider.Code).HasMaxLength(40).IsRequired();
        builder.Property(provider => provider.Name).HasMaxLength(160).IsRequired();
        builder.HasIndex(provider => provider.Code).IsUnique();
        builder.HasIndex(provider => new { provider.IsActive, provider.Name });
    }
}

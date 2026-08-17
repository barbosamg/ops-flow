using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Customers;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Name).HasMaxLength(160).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(customer => customer.Email).IsUnique();
        builder.HasIndex(customer => new { customer.IsActive, customer.Name });
    }
}

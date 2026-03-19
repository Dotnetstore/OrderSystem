using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Customer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status)
            .HasConversion<string>() // Enum → string
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // --- AuditLog as owned collection ---
        builder.OwnsMany(o => o.AuditLog, audit =>
        {
            audit.ToTable("OrderAuditLogs");

            audit.WithOwner().HasForeignKey("OrderId");

            audit.Property<string>("Message")
                .HasColumnName("Message")
                .IsRequired();

            audit.Property<DateTime>("Timestamp")
                .HasColumnName("Timestamp")
                .IsRequired();

            audit.HasKey("OrderId", "Timestamp"); // Composite key
        });

        // --- Ignore domain events ---
        builder.Ignore(o => o.DomainEvents);
    }
}
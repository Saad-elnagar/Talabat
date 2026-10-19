using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Domain.Entities.OrderAggregate;

namespace Talabat.Infrastructure.Persistence.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // ShippingAddress = Value Object (ملهاش table منفصل) → OwnsOne صح
            builder.OwnsOne(o => o.ShippingAddress,
                shippingAddress => shippingAddress.WithOwner());

            // DeliveryMethod = Entity منفصلة (لها DbSet) → HasOne
            builder.HasOne(o => o.DeliveryMethod)
                   .WithMany()
                   .HasForeignKey(o => o.DeliveryMethodId);

            // Enum → String
            builder.Property(o => o.OrderStatus)
                .HasConversion(
                    s => s.ToString(),
                    s => (OrderStatus)Enum.Parse(typeof(OrderStatus), s)
                );

            // Decimal
            builder.Property(o => o.Subtotal)
                .HasPrecision(18, 2);
        }
    }
}
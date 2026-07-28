using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Reposatory.Data.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
           
            builder.OwnsOne(o => o.ShippingAddress,
                shippingAddress => shippingAddress.WithOwner());

            
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Reposatory.Data.Config
{
    public class OrderIteamConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {

            builder.OwnsOne(o => o.Product, Product => Product.WithOwner());

            builder.Property(o => o.Price)
                .HasColumnType("decimal(18,2)");
        }


    }
}

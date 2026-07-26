using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Reposatory.Data.Config
{
    public class DelivaryMehodConfig : IEntityTypeConfiguration<DeliveryMethod>
    {
        void IEntityTypeConfiguration<DeliveryMethod>.Configure(EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.Property(d => d.Cost)
                .HasColumnType("decimal(18,2)");

        }
    }
}

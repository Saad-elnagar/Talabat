using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities.OrderAggregate;

namespace Talabat.Infrastructure.Persistence.Config
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

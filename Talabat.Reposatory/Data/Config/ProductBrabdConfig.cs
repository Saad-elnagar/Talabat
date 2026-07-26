using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entitys;

namespace Talabat.Reposatory.Data.Config
{
    public class ProductBrabdConfig : IEntityTypeConfiguration<ProductBrand>
    {
        public void Configure(EntityTypeBuilder<ProductBrand> builder)
        {
            builder.Property(b => b.Name).IsRequired();

        }
    }
}

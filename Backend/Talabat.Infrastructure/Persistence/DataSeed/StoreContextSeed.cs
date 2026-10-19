using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Domain.Entities;
using Talabat.Domain.Entities.OrderAggregate;
using Talabat.Infrastructure.Persistence.Config;

namespace Talabat.Infrastructure.Persistence.DataSeed
{
    public static class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext context)
        {
            var seedDirectory = Path.Combine(AppContext.BaseDirectory, "DataSeed");
            var brandData = File.ReadAllText(Path.Combine(seedDirectory, "brands.json"));
            var typeData = File.ReadAllText(Path.Combine(seedDirectory, "types.json"));
            var productData = File.ReadAllText(Path.Combine(seedDirectory, "products.json"));
            var delivery = File.ReadAllText(Path.Combine(seedDirectory, "delivery.json"));

            if (!context.ProductBrands.Any())
            {
                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);
                foreach (var item in brands)
                {
                    context.ProductBrands.Add(item);
                }
                await context.SaveChangesAsync();
            }

            if (!context.ProductTypes.Any())
            {
                var types = JsonSerializer.Deserialize<List<ProductType>>(typeData);
                foreach (var item in types)
                {
                    context.ProductTypes.Add(item);
                }
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var products = JsonSerializer.Deserialize<List<Product>>(productData);
                foreach (var item in products)
                {
                    context.Products.Add(item);
                }
                await context.SaveChangesAsync();

            }
            if (!context.DeliveryMethods.Any())
            {
                var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(delivery);
                foreach (var item in deliveryMethods)
                {
                    context.DeliveryMethods.Add(item);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

                
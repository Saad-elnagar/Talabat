using System.Text.Json;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Reposatory.Data.DataSeed
{
    public static class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext context)
        {
            var brandData = File.ReadAllText("../Talabat.Reposatory/Data/DataSeed/brands.json");
            var typeData = File.ReadAllText("../Talabat.Reposatory/Data/DataSeed/types.json");
            var productData = File.ReadAllText("../Talabat.Reposatory/Data/DataSeed/products.json");
            var delivery = File.ReadAllText("../Talabat.Reposatory/Data/DataSeed/delivery.json");

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


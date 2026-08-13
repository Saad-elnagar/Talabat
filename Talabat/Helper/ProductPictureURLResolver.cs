using AutoMapper;
using Talabat.Core.Entitys;
using Talabat.Dto;
using Product = Talabat.Core.Entitys.Product;

namespace Talabat.Helper
{
    public class ProductPictureURLResolver : IValueResolver<Product, ProductResultToDto, string>
    {
        private readonly IConfiguration _config;
        public ProductPictureURLResolver(IConfiguration config)
        {
            _config = config;

        }
        public string Resolve(Product source, ProductResultToDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PictureUrl))
            {
                // BUG FIX: previously this concatenated ApiBaseURL and PictureUrl with no
                // separator (e.g. "http://localhost:5208" + "images/products/x.png" =
                // "http://localhost:5208images/products/x.png" — an invalid URL, so every
                // product image failed to load). TrimEnd/TrimStart guarantees exactly one "/"
                // between them regardless of whether either value already has one.
                var baseUrl = (_config["ApiBaseURL"] ?? string.Empty).TrimEnd('/');
                var relativePath = source.PictureUrl.TrimStart('/');
                return $"{baseUrl}/{relativePath}";
            }
            return string.Empty;
        }
    }
}

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
                return $"{_config["ApiBaseURL"]}{source.PictureUrl}";
            }
            return string.Empty;
        }
    }
}

using AutoMapper;
using Microsoft.Extensions.Configuration;
using Talabat.Domain.Entities;
using Talabat.Application.DTOs;

namespace Talabat.Application.Mapping
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
                var baseUrl = _config["ApiBaseURL"]?.TrimEnd('/');
                return string.IsNullOrEmpty(baseUrl)
                    ? source.PictureUrl
                    : $"{baseUrl}/{source.PictureUrl.TrimStart('/')}";
            }
           return string.Empty;
        }
    }
}

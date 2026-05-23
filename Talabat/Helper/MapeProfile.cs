using AutoMapper;
using Talabat.Core.Entitys;
using Talabat.Dto;

namespace Talabat.Helper
{
    public class MapeProfile : Profile
    {
        public MapeProfile()
        {
            CreateMap<Product, ProductResultToDto>()
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrand.Name))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.Name))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<ProductPictureURLResolver>());
        }
    }
}

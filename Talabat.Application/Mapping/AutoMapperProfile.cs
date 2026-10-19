using AutoMapper;
using Talabat.Domain.Entities;
using Talabat.Domain.Entities.Identity;
using Talabat.Application.DTOs;

namespace Talabat.Application.Mapping
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            CreateMap<Product, ProductResultToDto>()
                .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrand.Name))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.Name))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<ProductPictureURLResolver>());

            CreateMap<Address, AddressDto>().ReverseMap();



            CreateMap<AddressDto, Talabat.Domain.Entities.OrderAggregate.Address>();
                
                

        }
    }
}

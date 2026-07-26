using Address = Talabat.Core.Entitys.Order_Aggregate.Address;
using Product = Talabat.Core.Entitys.Product;

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

            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<CustomerBasketDto, CustomerBasket>().ReverseMap();



            CreateMap<AddressDto, Talabat.Core.Entitys.Order_Aggregate.Address>();



        }
    }
}

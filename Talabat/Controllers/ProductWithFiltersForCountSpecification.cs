using Talabat.Core.Entitys;
using Talabat.Core.Specification;
using Talabat.Reposatory.Specification;

namespace Talabat.Controllers
{
    public class ProductWithFiltersForCountSpecification : Specification<Product>
    {

        public ProductWithFiltersForCountSpecification(ProductSpecParams specParams) : base(p =>
              (string.IsNullOrEmpty(specParams.search) || p.Name.ToLower().Contains(specParams.search)) &&
            (!specParams.Needprice.HasValue || p.Price == specParams.Needprice.Value)&&
            (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId.Value) &&
            (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId.Value))
        {

        }

    }
}
using Talabat.Core.Entitys;
using Talabat.Core.Specification;
using Talabat.Reposatory.Specification;

namespace Talabat.Controllers
{
    public class ProductWithFiltersForCountSpecification : Specification<Product>
    {

        public ProductWithFiltersForCountSpecification(ProductSpecParams specParams) : base(p =>
            (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId.Value) &&
            (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId.Value))
        {

        }

    }
}
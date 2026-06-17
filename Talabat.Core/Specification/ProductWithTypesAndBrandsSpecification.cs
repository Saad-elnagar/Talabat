using System.Linq;
using Talabat.Core.Entitys;
using Talabat.Reposatory.Specification;

namespace Talabat.Core.Specification
{
    public class ProductWithTypesAndBrandsSpecification : Specification<Product>
    {
        public ProductWithTypesAndBrandsSpecification(ProductSpecParams specParams)
        {
            Criteria = p =>
                 (string.IsNullOrEmpty(p.search) || p.Name.ToLower().Contains(p.search)) &&
                (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId.Value) &&
                (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId.Value);
            Includes.Add(p => p.ProductBrand);
            Includes.Add(p => p.ProductType);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "priceAsc":
                        AddOrderBy(p => p.Price);
                        break;
                    case "priceDesc":
                        AddOrderByDescending(p => p.Price);
                        break;
                    default:
                        AddOrderBy(p => p.Name);
                        break;
                }
            }
            else
            {
                AddOrderBy(p => p.Name); 
            }
            ApplyPagination((specParams.PageSize.Value) * (specParams.PageIndex.Value - 1), specParams.PageSize.Value);
        }

    
        public ProductWithTypesAndBrandsSpecification(int id) : base(p => p.Id == id)
        {
            Includes.Add(p => p.ProductBrand);
            Includes.Add(p => p.ProductType);
        }
    }
}
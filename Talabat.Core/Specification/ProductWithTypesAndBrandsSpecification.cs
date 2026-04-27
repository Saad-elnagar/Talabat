using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Reposatory.Specification;

namespace Talabat.Core.Specification
{
    public class ProductWithTypesAndBrandsSpecification : Specification<Product>
    {
        public ProductWithTypesAndBrandsSpecification()
        {
            Includes.Add(P => P.ProductBrand);
            Includes.Add(P => P.ProductType);
        }
        public ProductWithTypesAndBrandsSpecification(int id) : base(p => p.Id == id)
        {
            Includes.Add(P => P.ProductBrand);
            Includes.Add(P => P.ProductType);


        }
    }
}

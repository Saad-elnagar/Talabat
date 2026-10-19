using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities;

namespace Talabat.Domain.Specifications
{
    public class ProductsByIdsSpec : Specification<Product>
    {
        public ProductsByIdsSpec(List<int> productIds)  
        
           : base(p => productIds.Contains(p.Id))
        {


        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Reposatory.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Talabat.Core.Specification
{
    public class ProductsByIdsSpec : Specification<Product>
    {
        public ProductsByIdsSpec(List<int> productIds)
        
           : base(p => productIds.Contains(p.Id))
        {


        }
    }
}

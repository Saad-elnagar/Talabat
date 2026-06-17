using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entitys
{
    public class Product : Base
    {
        public string? search;

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureUrl { get; set; }
        public int ProductTypeId { get; set; } 

        public int ProductBrandId { get; set; }
        public ProductType ProductType { get; set; } // Navigation property to ProductType
        public ProductBrand ProductBrand { get; set; } // Navigation property to ProductBrand   

    }
}

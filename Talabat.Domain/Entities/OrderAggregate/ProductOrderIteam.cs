using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Domain.Entities.OrderAggregate
{
    public class ProductOrderIteam
    {
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string? PictureUrl { get; set; }

    }
}

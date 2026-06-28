using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entitys.Order_Aggregate
{
    public class OrderItem : BaseEntity
    {
        public OrderItem() { }
        public OrderItem(ProductOrderIteam product, decimal price, int quantity)
        {
            Product = product;
            Price = price;
            Quantity = quantity;  
       }
        


        public ProductOrderIteam Product { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }
}

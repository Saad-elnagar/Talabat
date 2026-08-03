

namespace Talabat.Core.Entitys.Order_Aggregate
{
    public class OrderItem : BaseEntity
    {
        public OrderItem() { }
        public OrderItem(ProductOrderIteam product, decimal price, int quantity)
        {            Product = product;
            Price = price;
            Quantity = quantity;  
       }
        


        public ProductOrderIteam Product { get; set; }
     
        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }
}

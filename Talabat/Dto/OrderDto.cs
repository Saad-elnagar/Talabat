using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Dto
{
    public class OrderDto
    {



       public string buyerEmail{ get; set; }

       public int deliveryMethodId{ get; set; }

       public string basketId{ get; set; }

       public Address shippingAddress{ get; set; }
         
        public OrderStatus orderStatus{ get; set; }


    }
}

using Talabat.Domain.Entities.OrderAggregate;

namespace Talabat.Application.DTOs
{
    public class OrderDto
    {



       public string buyerEmail{ get; set; }

       public int deliveryMethodId{ get; set; }

       public string basketId{ get; set; }

       public Address shippingAddress{ get; set; }


    }
}

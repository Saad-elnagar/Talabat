using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Core.Entitys
{
    public class CustomerBasket
    {
       public string  Id { get; set; }
        public List<BasketIteam> Items { get; set; } 
        public string? PaymentId { get; set; }

        public string ClientSecret { get; set; }
        public int? DeliveryMethodId { get; set; }
        public decimal? ShippingPrice { get; set; }

        public string? SessionId { get; set; }
        public Talabat.Core.Entitys.Order_Aggregate.Address ShippingAddress { get; set; }
        public string BuyerEmail { get; set; }
    }
}

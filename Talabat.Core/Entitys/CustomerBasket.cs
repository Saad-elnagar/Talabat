using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entitys
{
    public class CustomerBasket
    {
       public string  Id { get; set; }
        public List<BasketIteam> Items { get; set; } 
        public string PaymentIntentId { get; set; }

        public string ClientSecret { get; set; }
        public int? DeliveryMethodId { get; set; }
        public decimal ShippingPrice { get; set; }
    }
}

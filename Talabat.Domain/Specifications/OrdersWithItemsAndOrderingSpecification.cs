using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities.OrderAggregate;

namespace Talabat.Domain.Specifications
{
    public class OrdersWithItemsAndOrderingSpecification : Specification<Talabat.Domain.Entities.OrderAggregate.Order>
    {
        public OrdersWithItemsAndOrderingSpecification(string buyerEmail) : base(o => o.BuyerEmail == buyerEmail)
        {
            Includes.Add(o => o.Items);
        }
        public OrdersWithItemsAndOrderingSpecification(string buyerEmail, int orderid) : base(o => o.BuyerEmail == buyerEmail && o.Id == orderid)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.ShippingAddress);
            Includes.Add(o => o.DeliveryMethod);

        }

    }
}

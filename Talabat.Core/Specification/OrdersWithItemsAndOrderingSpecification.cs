using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Reposatory.Specification;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Core.Specification
{
    public class OrdersWithItemsAndOrderingSpecification : Specification<Talabat.Core.Entitys.Order_Aggregate.Order>
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

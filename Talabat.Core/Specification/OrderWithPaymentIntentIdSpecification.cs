
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Reposatory.Specification;

namespace Talabat.Core.Specification
{
    public class OrderWithPaymentIntentIdSpecification :Specification<Order>
    {

        public OrderWithPaymentIntentIdSpecification(string paymentIntentId)
            : base(o => o.PaymentIntentId == paymentIntentId)
        {
        }
    
    }
}

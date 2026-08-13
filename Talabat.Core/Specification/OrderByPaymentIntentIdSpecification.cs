using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Reposatory.Specification;

namespace Talabat.Core.Specification
{
    public class OrderByPaymentIntentIdSpecification : Specification<Order>
    {
        public OrderByPaymentIntentIdSpecification(string paymentIntentId)
            : base(o => o.PaymentIntentId == paymentIntentId)
        {
            Includes.Add(o => o.Items);
        }
    }
}

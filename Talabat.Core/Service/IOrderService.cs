using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Core.Service
{
    public  interface IOrderService
    {

        Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress);

        Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
        Task<Order>GetOrderByIdSpecificAsync(string buyerEmail, int orderId);

        Task<Order> CancelOrder(string buyerEmail, int orderId);
        Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync();

        /// <summary>
        /// Called by the Stripe webhook handler once a payment_intent event is verified.
        /// Looks up the order by its PaymentId and updates OrderStatus accordingly.
        /// Returns null if no matching order is found (e.g. event for an order not yet created).
        /// </summary>
        Task<Order?> UpdateOrderPaymentStatusAsync(string paymentId, bool paymentSucceeded);



    }
}

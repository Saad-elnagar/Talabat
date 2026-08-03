using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Core.Service
{
    public  interface IOrderService
    {

        Task <Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress , OrderStatus orderStatus );

        Task<Order> CancelOrderAsync(string buyerEmail, int orderId);

        Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
        Task<Order>GetOrderByIdSpecificAsync(string buyerEmail, int orderId);

        Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync();



    }
}

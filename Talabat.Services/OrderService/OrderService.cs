// Talabat.Services/OrderService/OrderService.cs
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Talabat.Core.Specification;

namespace Talabat.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepository, IUnitOfWork unitOfWork)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId,
            string basketId, Address shippingAddress,OrderStatus orderStatus )
        {
            // 1. يجيب السلة من Redis
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket == null) return null;

            // 2. يجيب طريقة التوصيل
            var deliveryMethod = await _unitOfWork
                .Repository<DeliveryMethod>()
                .GetByIdAsync(deliveryMethodId);
            if (deliveryMethod == null) return null;

            // 3. يحسب المجموع
            var subtotal = basket.Items.Sum(i => i.Price * i.Quantity);

            // 4. يحول BasketItem → OrderItem
            var orderItems = basket.Items.Select(item => new OrderItem(
                new ProductOrderIteam
                {
                    ProductName = item.ProductName,
                     PictureUrl= item.PictureUrl
                }, 
                item.Price,
                item.Quantity
            )).ToList();

            // 5. يعمل الطلب
            var order = new Order(
                buyerEmail: buyerEmail,
                orderStatus: orderStatus,
                shippingAddress: shippingAddress,
                deliveryMethod: deliveryMethod,
                items: orderItems,         
                subtotal: subtotal,
                paymentIntentId: basket.PaymentId ?? ""
            );

            // 6. يحفظ
            _unitOfWork.Repository<Order>().Add(order);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;

            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);
            return await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
        }

        public async Task<Order> GetOrderByIdSpecificAsync(string buyerEmail, int orderId)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail, orderId);
            return await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec );
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
        }
    }
}
// Talabat.Services/OrderService/OrderService.cs
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Exceptions;
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
            string basketId, Address shippingAddress, OrderStatus orderStatus)
        {

            // Get Basket 

            var basket = await _basketRepository.GetBasketAsync(basketId);



            if (basket == null) return null;



            // Get items from the product repository
            var productRepo = _unitOfWork.Repository<Product>();
            var products = new List<Product>();

            foreach (var item in basket.Items)
            {
                var product = await productRepo.GetByIdAsync(item.Id)
                    ?? throw new ProductNotFoundException(item.Id);

                if (product.StockQuantity < item.Quantity)
                    throw new InsufficientStockException(product.Name, item.Quantity, product.StockQuantity);

                products.Add(product);
            }

            // Get delivery method from the repository
            var deliveryMethod = await _unitOfWork
                .Repository<DeliveryMethod>()
                .GetByIdAsync(deliveryMethodId);
            if (deliveryMethod == null) return null;


            var subtotal = basket.Items.Sum(i => i.Price * i.Quantity);

            
            var orderItems = basket.Items.Select(item => new OrderItem(
                new ProductOrderIteam
                {
                    ProductName = item.ProductName,
                    PictureUrl = item.PictureUrl
                },
                item.Price,
                item.Quantity
            )).ToList();


            var order = new Order(
                buyerEmail: buyerEmail,
                orderStatus: orderStatus,
                shippingAddress: shippingAddress,
                deliveryMethod: deliveryMethod,
                items: orderItems,
                subtotal: subtotal,
                paymentIntentId: basket.PaymentId ?? ""
            );


            _unitOfWork.Repository<Order>().Add(order);

            foreach (var item in basket.Items)
            {
                var product = products.First(p => p.Id == item.Id);
                product.StockQuantity -= item.Quantity;
                _unitOfWork.Repository<Product>().Update(product);
            }

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
            return await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
        }

        public async Task<Order> CancelOrderAsync(string buyerEmail, int orderId)
        {
            // Retrieve the order for the user
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail, orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);
            if (order == null) return null;
            if (order.OrderStatus != OrderStatus.Pending)
                throw new InvalidOrderStateException(order.Id, order.OrderStatus);

            // Update the order status to Cancelled
            order.OrderStatus = OrderStatus.Cancelled;

            var productRepo = _unitOfWork.Repository<Product>();
            foreach (var item in order.Items)
            {
                var product = await productRepo.GetByIdAsync(item.Product.ProductId);
                if (product == null) continue;

                product.StockQuantity += item.Quantity;
                productRepo.Update(product);
            }
            _unitOfWork.Repository<Order>().Update(order);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) return null;
            return order;

        }
    }
}
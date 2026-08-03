// Talabat.Services/OrderService/OrderService.cs
using Microsoft.Extensions.Logging;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Exceptions;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Talabat.Core.Specification;
using Talabat.Reposatory.Specification;

namespace Talabat.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

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

            var deliveryMethod = await _unitOfWork
                .Repository<DeliveryMethod>()
                .GetByIdAsync(deliveryMethodId);
            if (deliveryMethod == null) return null;


            // 1. Load and lock in the products referenced by the basket, then validate stock.
            //    Throwing here (instead of returning null) lets callers distinguish
            //    "no stock" from other generic failures and surface a clear message.
            //
            //    Fetched in a SINGLE query (WHERE Id IN (...)) instead of one query per
            //    basket item, to avoid the N+1 problem: with a 10-item basket, looping
            //    with GetByIdAsync would mean 10 separate round trips to the database.
            var productRepo = _unitOfWork.Repository<Product>();
            var productIds = basket.Items.Select(i => i.Id).ToHashSet();
            var productsSpec = new Specification<Product>(p => productIds.Contains(p.Id)); 
            var products = (await productRepo.GetAllWithSpecAsync(productsSpec)).ToList();



            foreach (var item in basket.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.Id)
                    ?? throw new ProductNotFoundException(item.Id); 

                if (product.StockQuantity < item.Quantity)
                    throw new InsufficientStockException(product.Name, item.Quantity, product.StockQuantity);
            }


            var subtotal = basket.Items.Sum(i => i.Price * i.Quantity);


            var orderItems = basket.Items.Select(item => new OrderItem(
                new ProductOrderIteam
                {
                    ProductId = item.Id,
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

            // 2. Decrement stock for each product in the same SaveChanges call as the
            //    order insert, so the order and the stock deduction commit together.
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

        public async Task<Order> CancelOrderAsync(string buyerEmail, int orderId)
        {
            
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail, orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);

            if (order == null) return null;

            if (order.OrderStatus != OrderStatus.Pending)
                throw new InvalidOrderStateException(order.Id, order.OrderStatus);

            // Give every unit back to stock, in the same SaveChanges as the status
            // change, so "order cancelled" and "stock restored" happen atomically.
            var productRepo = _unitOfWork.Repository<Product>();
            foreach (var item in order.Items)
            {
                var product = await productRepo.GetByIdAsync(item.Product.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} referenced by cancelled order {OrderId} no longer exists; stock not restored.", item.Product.ProductId, order.Id);
                    continue;
                } // product may have been deleted since the order was placed

                product.StockQuantity += item.Quantity;
                productRepo.Update(product);
            }

            order.OrderStatus = OrderStatus.Cancelled;
            _unitOfWork.Repository<Order>().Update(order);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) return null;

            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
        }
    }
}
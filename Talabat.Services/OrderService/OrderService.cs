using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Talabat.Core.Specification;

namespace Talabat.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepo;

        public OrderService(IUnitOfWork unitOfWork, IBasketRepository basketRepo)
        {
            _unitOfWork = unitOfWork;
            _basketRepo = basketRepo;
        }
       public async  Task <Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {

            var basket = await _basketRepo.GetBasketAsync(basketId);

            if (basket?.Items?.Any() != true)
                throw new InvalidOperationException("Basket is empty.");

          
            var productIds = basket.Items.Select(i => i.Id).ToList();
            var spec = new ProductsByIdsSpec(productIds);

            var products = await _unitOfWork.Repository<Product>()
                .ListAsync(spec);

            var productDict = products.ToDictionary(p => p.Id);

            var orderItems = basket.Items
                .Where(item => productDict.TryGetValue(item.Id, out _))
                .Select(item =>
                {
                    var product = productDict[item.Id];
                    var productItem = new ProductOrderIteam
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        PictureUrl=product.PictureUrl,
                    };
                    return new OrderItem(productItem, product.Price, item.Quantity);
                })
                .ToList();

            if (!orderItems.Any())
                throw new InvalidOperationException("No valid products found.");

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                .GetByIdAsync(deliveryMethodId)
                ?? throw new ArgumentException("Invalid delivery method.");

            var subtotal = orderItems.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                BuyerEmail = buyerEmail,
                ShippingAddress = shippingAddress,
                DeliveryMethod = deliveryMethod,
                Items = orderItems,
                Subtotal = subtotal
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            return order;
        }






        

        public Task<Order> GetOrderByIdSpecificAsync(string buyerEmail, int orderId)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail, orderId);
            return _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);
        }

        public Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            var orders = _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);

            return orders;


        }
        public async Task<IReadOnlyList<DeliveryMethod>> GeAllDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();

            return deliveryMethods;

        }
    }
}

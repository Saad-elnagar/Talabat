
namespace Talabat.Helper
{
    public class OrderMapper : IOrderMapper
    {
        public OrderItemDto MapToOrderItemDto(OrderItem item)
        {
            if (item?.Product == null) return null;

            return new OrderItemDto
            {
                ProductName = item.Product.ProductName,
                //   ProductId = item.Product.ProductId,
                Price = item.Price,
                PictureUrl = item.Product.PictureUrl,
                Quantity = item.Quantity
            };
        }

        public UserOrderDto MapToUserOrderDto(Order order)
        {
            if (order == null) return null;

            return new UserOrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus.ToString(),
                Subtotal = order.Subtotal,
                Items = order.Items?
                    .Select(MapToOrderItemDto)
                    .Where(i => i != null)
                    .ToList() ?? new List<OrderItemDto>()
            };
        }

        public UserOrdersSummaryDto MapToSummaryDto(string buyerEmail, IReadOnlyList<Order> orders)
        {
            if (orders == null || !orders.Any())
                return new UserOrdersSummaryDto { BuyerEmail = buyerEmail };

            var orderDtos = orders
                .Select(MapToUserOrderDto)
                .Where(o => o != null)
                .ToList();

            return new UserOrdersSummaryDto
            {
                BuyerEmail = buyerEmail,
                TotalOrders = orderDtos.Count,
                TotalSpent = CalculateTotalSpent(orders),
                Orders = orderDtos
            };
        }

        public UserOrdersSummaryDto MapToSingleOrderSummary(Order order)
        {
            if (order == null) return null;

            var orderDto = MapToUserOrderDto(order);

            return new UserOrdersSummaryDto
            {
                BuyerEmail = order.BuyerEmail,
                TotalOrders = 1,
                TotalSpent = order.Subtotal + (order.DeliveryMethod?.Cost ?? 0),
                Orders = new List<UserOrderDto> { orderDto }
            };
        }

        // SRP: Private helpers
        private string MapStatusToString(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.PaymentReceived => "Payment Received",
                OrderStatus.PaymentFailed => "Payment Failed",
                _ => "Unknown"
            };
        }

        private decimal CalculateTotalSpent(IReadOnlyList<Order> orders)
        {
            return orders.Sum(o => o.Subtotal + (o.DeliveryMethod?.Cost ?? 0));
        }
    }
}

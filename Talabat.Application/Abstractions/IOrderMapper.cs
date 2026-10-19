using Talabat.Domain.Entities.OrderAggregate;
using Talabat.Application.DTOs;

namespace Talabat.Application.Abstractions
{
    public interface IOrderMapper
    {
        OrderItemDto MapToOrderItemDto(OrderItem item);
        UserOrderDto MapToUserOrderDto(Order order);
        UserOrdersSummaryDto MapToSummaryDto(string buyerEmail, IReadOnlyList<Order> orders);
    }
}

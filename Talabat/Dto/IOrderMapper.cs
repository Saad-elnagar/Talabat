using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Dto
{
    public interface IOrderMapper
    {
        OrderItemDto MapToOrderItemDto(OrderItem item);
        UserOrderDto MapToUserOrderDto(Order order);
        UserOrdersSummaryDto MapToSummaryDto(string buyerEmail, IReadOnlyList<Order> orders);
    }
}

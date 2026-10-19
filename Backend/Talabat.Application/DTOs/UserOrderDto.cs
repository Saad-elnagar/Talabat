namespace Talabat.Application.DTOs
{

    public class UserOrderDto
    {
        public int Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal Subtotal { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
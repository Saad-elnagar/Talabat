

namespace Talabat.Core.Entitys.Order_Aggregate
{
    public class Order : BaseEntity
    {
        public Order() { }

        public Order(string buyerEmail, OrderStatus orderStatus, Address shippingAddress,
                     DeliveryMethod? deliveryMethod, ICollection<OrderItem> items,
                     decimal subtotal, string paymentIntentId)
        {
            BuyerEmail = buyerEmail;
            OrderStatus = orderStatus;
            ShippingAddress = shippingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            Subtotal = subtotal;
            PaymentIntentId = paymentIntentId;
        }

        public string BuyerEmail { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public DateTimeOffset OrderDate { get; set; } = DateTime.Now;

        public Address ShippingAddress { get; set; }

        public int? DeliveryMethodId { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public decimal Subtotal { get; set; }

        public decimal GetTotal() => Subtotal + (DeliveryMethod?.Cost ?? 0);

        public string PaymentIntentId { get; set; } 
    }
}
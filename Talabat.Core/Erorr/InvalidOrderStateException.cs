using Talabat.Core.Entitys.Order_Aggregate;

namespace Talabat.Core.Exceptions
{
    public class InvalidOrderStateException : Exception
    {
        public int OrderId { get; }
        public OrderStatus CurrentStatus { get; }

        public InvalidOrderStateException(int orderId, OrderStatus currentStatus)
            : base($"Order {orderId} cannot be cancelled because it is currently '{currentStatus}'. Only orders with status 'Pending' can be cancelled.")
        {
            OrderId = orderId;
            CurrentStatus = currentStatus;
        }
    }
}
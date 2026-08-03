using System.Runtime.Serialization;


namespace Talabat.Core.Entitys.Order_Aggregate
{
    public enum OrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "PaymentReceived")]
        PaymentReceived,
        [EnumMember(Value = "PaymentFailed")]
        PaymentFailed,
        [EnumMember(Value = "Refunded")]
        Refunded,
        [EnumMember(Value = "Cancelled")]
        Cancelled
    }
}
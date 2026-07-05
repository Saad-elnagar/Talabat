namespace Talabat.Dto
{
    public class UserOrdersSummaryDto
    {
        public string BuyerEmail { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<UserOrderDto> Orders { get; set; } = new();
    }
}

namespace Talabat.Application.DTOs
{
    public class DeliveryMethodDto
    {
        public int Id { get; set;   }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string DeliveryTime { get; set; }
    }
}

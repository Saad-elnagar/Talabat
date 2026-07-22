using Stripe;
using System.ComponentModel.DataAnnotations;

namespace Talabat.Dtos
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; }

        public string?PaymentIntentId { get; set; }
        public decimal? ShippingPrice { get; set; }
        public int? DeliveryMethodId { get; set; }
        public Talabat.Core.Entitys.Order_Aggregate.Address ShippingAddress { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Basket must contain at least one item")]
        public List<BasketItemDto> Items { get; set; } = new();
    }
}

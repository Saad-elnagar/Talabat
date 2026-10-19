using System.ComponentModel.DataAnnotations;

namespace Talabat.Application.DTOs
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Basket must contain at least one item")]
        public List<BasketItemDto> Items { get; set; } = new();
    }
}

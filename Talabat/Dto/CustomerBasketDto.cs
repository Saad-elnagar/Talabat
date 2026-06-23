using System.ComponentModel.DataAnnotations;

namespace Talabat.Dtos
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Basket must contain at least one item")]
        public List<BasketItemDto> Items { get; set; } = new();
    }

    public class BasketItemDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string ProductName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        public string PictureUrl { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
// Talabat.APIs/DTOs/SendPaymentLinkRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.DTOs
{
    public class SendPaymentLinkRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        public string BasketId { get; set; }
    }
}
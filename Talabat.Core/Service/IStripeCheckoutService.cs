// Talabat.Core/Services.Contract/IStripeCheckoutService.cs
using Talabat.Core.Entitys;

namespace Talabat.Core.Service
{
    public interface IStripeCheckoutService
    {
        Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string basketId);
    }

    public class CheckoutSessionResult
    {
        public string SessionId { get; set; }
        public string CheckoutUrl { get; set; }
    }
}
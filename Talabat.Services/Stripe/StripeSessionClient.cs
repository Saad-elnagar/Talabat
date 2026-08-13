// Talabat.Services/Stripe/StripeSessionClient.cs
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;

namespace Talabat.Services.Stripe
{
    /// <summary>
    /// Thin wrapper around Stripe's static Checkout Session SDK call.
    /// Exists purely so StripeCheckoutService can be unit-tested by mocking
    /// this interface instead of hitting Stripe's real API in tests.
    /// </summary>
    public class StripeSessionClient : IStripeSessionClient
    {
        public async Task<Session> CreateSessionAsync(SessionCreateOptions options)
        {
            var service = new SessionService();
            return await service.CreateAsync(options);
        }
    }
}


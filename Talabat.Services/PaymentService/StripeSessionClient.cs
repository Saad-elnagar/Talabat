using Stripe.Checkout;
using Talabat.Core.Service;

namespace Talabat.Services.PaymentService
{
    public class StripeSessionClient : IStripeSessionClient
    {
        public async Task<Session> CreateSessionAsync(SessionCreateOptions options)
        {
            var service = new SessionService();
            return await service.CreateAsync(options);
        }
    }
}

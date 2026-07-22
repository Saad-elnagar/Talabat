using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

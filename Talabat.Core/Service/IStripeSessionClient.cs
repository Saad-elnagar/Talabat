// Talabat.Core/Service/IStripeSessionClient.cs
namespace Talabat.Core.Service
{
    public interface IStripeSessionClient
    {
        Task<Stripe.Checkout.Session> CreateSessionAsync(Stripe.Checkout.SessionCreateOptions options);
    }
}
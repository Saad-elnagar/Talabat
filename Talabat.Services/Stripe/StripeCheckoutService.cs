
// Talabat.Services/Stripe/StripeCheckoutService.cs
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Product = Talabat.Core.Entitys.Product;

namespace Talabat.Services.Stripe
{
    public class StripeCheckoutService : IStripeCheckoutService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeSessionClient _sessionClient;

        public StripeCheckoutService(
            IConfiguration configuration,
            IBasketRepository basketRepository,
            IUnitOfWork unitOfWork,
            IStripeSessionClient sessionClient)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _sessionClient = sessionClient;
        }

        public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string basketId)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket == null)
                throw new Exception("Basket not found.");

            // Refresh delivery cost from the DB (never trust a client-supplied shipping price)
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                    .GetByIdAsync(basket.DeliveryMethodId.Value);
                basket.ShippingPrice = deliveryMethod.Cost;
            }

            // Refresh item prices from the DB (never trust client-supplied prices)
            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if (item.Price != product.Price)
                    item.Price = product.Price;
            }

            var lineItems = basket.Items.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(item.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName
                    }
                }
            }).ToList();

            if (basket.ShippingPrice > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(basket.ShippingPrice * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Shipping"
                        }
                    }
                });
            }

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                SuccessUrl = _configuration["StripeSettings:SuccessUrl"],
                CancelUrl = _configuration["StripeSettings:CancelUrl"],
                Metadata = new Dictionary<string, string>
                {
                    { "basketId", basketId }
                }
            };

            var session = await _sessionClient.CreateSessionAsync(options);

            // Store the PaymentIntent id on the basket now, so the webhook
            // (StripeWebhookController -> IOrderService.UpdateOrderPaymentStatusAsync)
            // can match "payment_intent.succeeded" events back to this order later.
            basket.PaymentId = session.PaymentIntentId;
            await _basketRepository.UpdateCustomerBasketAsync(basket);

            return new CheckoutSessionResult
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
    }
}

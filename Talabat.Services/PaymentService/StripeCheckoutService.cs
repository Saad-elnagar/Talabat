// Talabat.Services/PaymentService/StripeCheckoutService.cs
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Exceptions;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Product = Talabat.Core.Entitys.Product;

namespace Talabat.Services.PaymentService
{
    public class StripeCheckoutService : IStripeCheckoutService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeSessionClient _stripeSessionClient;

        private const int SessionExpiryHours = 23;
        private const int SessionExpiryExtraMinutes = 59;
        private const string Currency = "usd";
        private const string ShippingLineItemName = "Shipping & Delivery";

        public StripeCheckoutService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IBasketRepository basketRepository,
            IStripeSessionClient stripeSessionClient)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
            _stripeSessionClient = stripeSessionClient;

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
        }

        public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string basketId)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId)
                ?? throw new BasketNotFoundException(basketId);

            await UpdateShippingPriceAsync(basket);
            await UpdateItemPricesAsync(basket);

            var lineItems = BuildLineItems(basket);
            var options = BuildSessionOptions(basketId, lineItems);

            Session session;
            try
            {
                session = await _stripeSessionClient.CreateSessionAsync(options);
            }
            catch (StripeException ex)
            {
                throw new CheckoutSessionException("Failed to create Stripe checkout session.", ex);
            }

            basket.SessionId = session.Id;
            basket.PaymentId = session.PaymentIntentId;
            basket.ClientSecret = session.Url; 

            await _basketRepository.UpdateCustomerBasketAsync(basket);

            return new CheckoutSessionResult
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }

        private List<SessionLineItemOptions> BuildLineItems(CustomerBasket basket)
        {
            var lineItems = basket.Items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = Currency,
                    UnitAmount = (long)(item.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName ?? "Product",
                        Images = !string.IsNullOrEmpty(item.PictureUrl)
                            ? new List<string> { item.PictureUrl }
                            : null
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            if (basket.ShippingPrice > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = Currency,
                        UnitAmount = (long)(basket.ShippingPrice * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = ShippingLineItemName
                        }
                    },
                    Quantity = 1
                });
            }

            return lineItems;
        }

        private SessionCreateOptions BuildSessionOptions(string basketId, List<SessionLineItemOptions> lineItems)
        {
            return new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{_configuration["ApiBaseUrl"]}/api/payments/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_configuration["ApiBaseUrl"]}/api/payments/cancel",
                ExpiresAt = DateTime.UtcNow.AddHours(SessionExpiryHours).AddMinutes(SessionExpiryExtraMinutes),
                Metadata = new Dictionary<string, string>
                {
                    { "BasketId", basketId }
                }
            };
        }

        private async Task UpdateItemPricesAsync(CustomerBasket basket)
        {
            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);

                if (product != null && item.Price != product.Price)
                    item.Price = product.Price;
            }
        }

        private async Task UpdateShippingPriceAsync(CustomerBasket basket)
        {
            if (!basket.DeliveryMethodId.HasValue) return;

            var deliveryMethod = await _unitOfWork
                .Repository<DeliveryMethod>()
                .GetByIdAsync(basket.DeliveryMethodId.Value);

            if (deliveryMethod != null)
                basket.ShippingPrice = deliveryMethod.Cost;
        }
    }
}
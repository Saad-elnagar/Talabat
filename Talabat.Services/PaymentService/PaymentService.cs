using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Core.Entitys.Order_Aggregate;
using Talabat.Core.Reposatory;
using Talabat.Core.Service;
using Product = Talabat.Core.Entitys.Product;

namespace Talabat.Services.PaymentService
{
    public class PaymentService : IPaymentIntent
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration, IUnitOfWork unitOfWork, IBasketRepository basketRepository)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
        }

        

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            // Set the Stripe API key from the configuration
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

            var basket = await _basketRepository.GetBasketAsync(basketId);

            if (basket == null)
                throw new Exception("Basket not found.");


            // If the basket has a delivery method, retrieve its cost and update the basket's shipping price
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
                basket.ShippingPrice = deliveryMethod.Cost;
            }

            // Update the prices of the items in the basket to ensure they are current
            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if (item.Price != product.Price)
                    item.Price = product.Price;
            }


            /// Create or update the payment intent based on whether it already exists
            PaymentIntent intent; // Declare a variable to hold the payment intent
            var paymentIntentService = new PaymentIntentService(); // Create an instance of the PaymentIntentService to interact with Stripe's payment intents
            long totalAmountInCents = CalculateTotalAmountInCents(basket);
            // If the basket does not have a payment intent ID, create a new payment intent
            if (string.IsNullOrEmpty(basket.PaymentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = totalAmountInCents, // Calculate the total amount for the payment intent, including item prices and shipping cost
                    Currency = "usd",// Set the currency for the payment intent
                    PaymentMethodTypes = new List<string> { "card" } // Specify that the payment method type is a card
                };
                // Use the async Stripe API rather than the blocking sync call — avoids tying
                // up a thread pool thread while waiting on Stripe's network call.
                intent = await paymentIntentService.CreateAsync(options);
                basket.PaymentId = intent.Id;
                basket.ClientSecret = intent.ClientSecret;

            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = totalAmountInCents,
                };
                await paymentIntentService.UpdateAsync(basket.PaymentId, options);
            }

            await _basketRepository.UpdateCustomerBasketAsync(basket);

            return basket;

        }

    private  long CalculateTotalAmountInCents(CustomerBasket basket)
        {
           
            var itemsTotal = basket.Items.Sum(item => item.Quantity * (item.Price * 100));
            var shippingTotal = basket.ShippingPrice * 100;

            return (long)(itemsTotal + shippingTotal);
        }
    }
}

using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities;
using Talabat.Domain.Entities.OrderAggregate;
using Talabat.Domain.Repositories;
using Product = Talabat.Domain.Entities.Product;

namespace Talabat.Infrastructure.Services.Payments
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
                throw new KeyNotFoundException("Basket not found.");


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
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = totalAmountInCents, // Calculate the total amount for the payment intent, including item prices and shipping cost
                    Currency = "usd",// Set the currency for the payment intent
                    PaymentMethodTypes = new List<string> { "card" } // Specify that the payment method type is a card
                };
                intent =  paymentIntentService.Create(options); // Create the payment intent using the specified options
                basket.PaymentIntentId = intent.Id;
                basket.ClientSecret = intent.ClientSecret;

            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = totalAmountInCents,
                };
                 paymentIntentService.Update(basket.PaymentIntentId, options);
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

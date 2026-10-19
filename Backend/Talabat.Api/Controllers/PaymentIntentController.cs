using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Talabat.Domain.Repositories;
using Talabat.Application.DTOs;

namespace Talabat.Api.Controllers
{
    public class PaymentIntentController : BaseController
    {

        private readonly IPaymentIntent _paymentIntent;
        private readonly IConfiguration _configuration;

        public PaymentIntentController(IPaymentIntent paymentIntent, IConfiguration configuration)
        {
            _paymentIntent = paymentIntent;
            _configuration = configuration;
        }

        [HttpGet("config")]
        public IActionResult GetPaymentConfiguration()
        {
            var publishableKey = _configuration["StripeSettings:Publishablekey"];
            return Ok(new { publishableKey });
        }

        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            try
            {
                var basket = await _paymentIntent.CreateOrUpdatePaymentIntent(basketId);
                if (basket == null)
                    return BadRequest(new ProblemDetails { Title = "Problem with your basket" });
                return Ok(basket);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
        }

    }
}

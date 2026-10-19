using Microsoft.AspNetCore.Mvc;
using Talabat.Domain.Repositories;
using Talabat.Application.DTOs;

namespace Talabat.Api.Controllers
{
    public class PaymentIntentController : BaseController
    {

        private readonly IPaymentIntent _paymentIntent;
        public PaymentIntentController(IPaymentIntent paymentIntent)
        {
            _paymentIntent = paymentIntent;
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

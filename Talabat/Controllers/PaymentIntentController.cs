using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Reposatory;
using Talabat.Dtos;

namespace Talabat.Controllers
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
            var basket = await _paymentIntent.CreateOrUpdatePaymentIntent(basketId);
            if (basket == null) return BadRequest(new ProblemDetails { Title = "Problem with your basket" });
            return Ok(basket);
        }

    }
}

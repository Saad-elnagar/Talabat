using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.Core.Service;

namespace Talabat.Controllers
{
    /// <summary>
    /// Receives payment status events directly from Stripe. This is the only source of truth
    /// for "did the payment actually succeed" — the client-side checkout flow must never be
    /// trusted to mark an order as paid on its own.
    ///
    /// Configure this endpoint's URL (https://yourdomain/api/stripewebhook) in the Stripe
    /// dashboard, and put the signing secret it gives you into StripeSettings:WebhookSecret
    /// in appsettings / user-secrets / environment variables (never commit it).
    /// </summary>
    public class StripeWebhookController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IOrderService orderService,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("StripeSettings:WebhookSecret is not configured; rejecting webhook.");
                return StatusCode(500);
            }

            Event stripeEvent;
            try
            {
                // ConstructEvent verifies the Stripe-Signature header against the raw payload
                // using the webhook secret. This is what proves the request genuinely came from
                // Stripe and wasn't forged by a client trying to fake a successful payment.
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                return BadRequest();
            }

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentIntent(stripeEvent, paymentSucceeded: true);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntent(stripeEvent, paymentSucceeded: false);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            // Stripe retries on any non-2xx response, so always acknowledge once we've processed
            // (or deliberately ignored) the event.
            return Ok();
        }

        private async Task HandlePaymentIntent(Event stripeEvent, bool paymentSucceeded)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent == null)
            {
                _logger.LogWarning("Stripe event {EventId} had no PaymentIntent payload.", stripeEvent.Id);
                return;
            }

            var order = await _orderService.UpdateOrderPaymentStatusAsync(intent.Id, paymentSucceeded);

            if (order == null)
            {
                _logger.LogWarning(
                    "Received Stripe event for PaymentIntent {PaymentIntentId} but no matching order was found.",
                    intent.Id);
                return;
            }

            _logger.LogInformation(
                "Order {OrderId} payment status updated to {Status} via Stripe webhook.",
                order.Id, order.OrderStatus);
        }
    }
}

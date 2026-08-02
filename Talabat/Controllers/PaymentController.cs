using Talabat.Core.Exceptions;

namespace Talabat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IStripeCheckoutService _checkoutService;
        private readonly IEmailService _emailService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<PaymentsController> _logger;
        private readonly string _webhookSecret;

        public PaymentsController(
            IStripeCheckoutService checkoutService,
            IEmailService emailService,
            IOrderService orderService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IBasketRepository basketRepository,
            ILogger<PaymentsController> logger)
        {
            _checkoutService = checkoutService;
            _emailService = emailService;
            _orderService = orderService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
            _logger = logger;
            _webhookSecret = _configuration["StripeSettings:WebhookSecret"]
                ?? throw new InvalidOperationException("StripeSettings:WebhookSecret is not configured.");
        }

        //  [HttpPost("send-email-link")]
        //public async Task<IActionResult> SendPaymentLinkByEmail([FromBody] SendPaymentLinkRequest request)
        //{
        //    try
        //    {
        //        var session = await _checkoutService.CreateCheckoutSessionAsync(request.BasketId);

        //        if (string.IsNullOrEmpty(session.CheckoutUrl))
        //            return BadRequest(new { success = false, message = "Checkout URL is empty" });

        //        await _emailService.SendPaymentLinkAsync(request.Email, session.CheckoutUrl);

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Payment link sent to email",
        //            email = request.Email,
        //            checkoutUrl = session.CheckoutUrl
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send payment link for basket {BasketId}", request.BasketId);
        //        return BadRequest(new { success = false, message = "Unable to create payment link." });
        //    }
        //}

        [HttpPost("checkout-session/{basketId}")]
        public async Task<IActionResult> CreateCheckoutSession(string basketId)
        {
            try
            {
                var result = await _checkoutService.CreateCheckoutSessionAsync(basketId);
                return Ok(new { success = true, sessionId = result.SessionId, checkoutUrl = result.CheckoutUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create checkout session for basket {BasketId}", basketId);
                return BadRequest(new { success = false, message = "Unable to create checkout session." });
            }
        }

        [HttpGet("success")]
        public IActionResult Success([FromQuery] string session_id)
        {
            return Ok(new { message = "Payment successful.", sessionId = session_id });
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok(new { message = "Payment cancelled. You can try again." });
        }

        // ─────────────────────────────────────────────────────────
        // Stripe webhook — signature-verified entry point
        // ─────────────────────────────────────────────────────────
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                return BadRequest(new { success = false, message = "Invalid signature." });
            }

            _logger.LogInformation("Stripe webhook received: {Type} ({EventId})", stripeEvent.Type, stripeEvent.Id);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentFailed(stripeEvent);
                    break;

                case "charge.refunded":
                    await HandleRefund(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {Type}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            var paymentIntentId = session.PaymentIntentId;
            var basketId = session.Metadata != null && session.Metadata.TryGetValue("BasketId", out var id) ? id : null;

            if (string.IsNullOrEmpty(basketId))
            {
                _logger.LogError("checkout.session.completed missing BasketId metadata. SessionId: {SessionId}", session.Id);
                return;
            }

            // 1. Idempotency guard — skip if this order was already processed
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                var existingSpec = new OrderWithPaymentIntentIdSpecification(paymentIntentId);
                var existingOrder = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(existingSpec);
                if (existingOrder != null)
                {
                    _logger.LogInformation("Order already exists for PaymentIntent {PI}, skipping.", paymentIntentId);
                    return;
                }
            }

            // 2. Fetch Basket from Redis
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket == null)
            {
                _logger.LogError("Basket not found for completed session: {BasketId}", basketId);
                return;
            }

            basket.PaymentId = paymentIntentId;
            await _basketRepository.UpdateCustomerBasketAsync(basket);


            // 3. Extract Shipping Address (Fallback to Stripe Session if not present in Basket)
            var shippingAddress = basket.ShippingAddress;


            var stripeShipping = session.CollectedInformation?.ShippingDetails;

            if (shippingAddress == null && stripeShipping != null)
            {
                var fullNameParts = stripeShipping.Name?.Split(' ') ?? new[] { "Customer", "" };

                shippingAddress = new Core.Entitys.Order_Aggregate.Address
                {
                    FirstName = fullNameParts[0],
                    LastName = fullNameParts.Length > 1 ? string.Join(" ", fullNameParts.Skip(1)) : "",
                    Street = stripeShipping.Address?.Line1 ?? "N/A",
                    City = stripeShipping.Address?.City ?? "Cairo",
                    State = stripeShipping.Address?.State ?? "Egypt"
                };
            }

            if (shippingAddress == null)
            {
                _logger.LogError("Basket {BasketId} has no shipping address on file or in Stripe Session; cannot create order.", basketId);
                return;
            }

            // 4. Extract Buyer Email (Fallback to Stripe Customer Details if missing in Basket)
            var buyerEmail = !string.IsNullOrEmpty(basket.BuyerEmail)
                ? basket.BuyerEmail
                : session.CustomerDetails?.Email ?? session.CustomerEmail;

            if (string.IsNullOrEmpty(buyerEmail))
            {
                _logger.LogError("Basket {BasketId} has no buyer email; cannot create order.", basketId);
                return;
            }

            // 5. Create Order
            Order order;
            try
            {
                order = await _orderService.CreateOrderAsync(

                    buyerEmail: buyerEmail,
                    deliveryMethodId: basket.DeliveryMethodId
                        ?? throw new InvalidOperationException($"Basket {basketId} has no delivery method set."),
                    basketId: basketId,
                    shippingAddress: shippingAddress,
                    orderStatus: OrderStatus.PaymentReceived
                );
            }
            catch (InsufficientStockException ex)
            {
                // Customer already paid but stock ran out between checkout and webhook.
                // Don't let Stripe keep retrying this webhook (it would just fail again);
                // log loudly so the team can manually refund and follow up.
                _logger.LogError(ex, "Order creation failed due to insufficient stock for basket {BasketId} after successful payment. Manual refund required.", basketId);
                return;
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogError(ex, "Order creation failed, product missing for basket {BasketId} after successful payment. Manual refund required.", basketId);
                return;
            }

            if (order != null)
            {
                order.PaymentIntentId = paymentIntentId;
                await _unitOfWork.CompleteAsync();
                await _basketRepository.DeleteCustomerBasketAsync(basketId);

                _logger.LogInformation("Order {OrderId} created with PaymentIntent {PI}", order.Id, paymentIntentId);
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning("Payment failed for PaymentIntent: {PaymentId}", paymentIntent.Id);

            var spec = new OrderWithPaymentIntentIdSpecification(paymentIntent.Id);
            var order = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);

            if (order != null)
            {
                order.OrderStatus = OrderStatus.PaymentFailed;
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Order status updated to PaymentFailed: {OrderId}", order.Id);
            }
        }

        private async Task HandleRefund(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null || string.IsNullOrEmpty(charge.PaymentIntentId))
            {
                _logger.LogWarning("Refund event object or PaymentIntentId is null.");
                return;
            }

            _logger.LogInformation("Refund processed for Charge: {ChargeId}", charge.Id);

            var spec = new OrderWithPaymentIntentIdSpecification(charge.PaymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(spec);

            if (order != null)
            {
                order.OrderStatus = OrderStatus.Refunded;
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Order refunded: {OrderId} | Status: {Status}", order.Id, order.OrderStatus);
            }
        }
    }
}
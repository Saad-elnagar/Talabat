

using Talabat.Core.Exceptions;

namespace Talabat.Controllers
{
    [Authorize(Policy = "JwtOnly")]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderMapper _orderMapper;

        public OrderController(IOrderService orderService, IOrderMapper orderMapper)
        {
            _orderService = orderService;
            _orderMapper = orderMapper;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto orderDto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var address = orderDto.shippingAddress;

            var order = await _orderService.CreateOrderAsync(
                email!,
                orderDto.deliveryMethodId,
                orderDto.basketId,
                address
               
            );

            if (order == null)
                return BadRequest(new APIRespose(400, "Problem creating order"));

            var result = _orderMapper.MapToUserOrderDto(order);
            return Ok(result);
        }



        [HttpGet]
        public async Task<ActionResult<UserOrdersSummaryDto>> GetOrdersForUser()
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrdersForUserAsync(email);

            if (orders == null || !orders.Any())
                return NotFound(new APIRespose(404, "No orders found for this user."));

            var result = _orderMapper.MapToSummaryDto(email, orders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserOrdersSummaryDto>> GetOrderById(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var order = await _orderService.GetOrderByIdSpecificAsync(email!, id);

            if (order == null)
                return NotFound(new APIRespose(404, "Order not found."));

            var result = _orderMapper.MapToUserOrderDto(order);
            return Ok(result);
        }

        [HttpGet("deliverymethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodDto>>> GetAllDelivaryMehod()
        {
            var deliveryMethods = await _orderService.GeAllDeliveryMethodsAsync();
            if (deliveryMethods == null || !deliveryMethods.Any())
                return NotFound(new APIRespose(404, "No delivery methods found."));
           
            var result = deliveryMethods.Select(dm => new DeliveryMethodDto
            {
                Id = dm.Id,
                ShortName = dm.ShortName,
                Description = dm.Description,
                Cost = dm.Cost,
                DeliveryTime = dm.DeliveryTime
            }).ToList();

            return Ok(result);

        }
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<OrderDto>> CancelOrder(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            Order order;
            try
            {
                order = await _orderService.CancelOrder(email!, id);
            }
            catch (InvalidOrderStateException ex)
            {
                return BadRequest(new APIRespose(400, ex.Message));
            }

            if (order == null)
                return NotFound(new APIRespose(404, "Order not found."));

            var result = _orderMapper.MapToUserOrderDto(order);
            return Ok(result);
        }
    }
}

namespace Talabat.Controllers
{
    public class BasketController : BaseController
    {
        private readonly IBasketRepository _basketRepository;

        public BasketController(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { Message = "Basket ID is required" });

            var basket = await _basketRepository.GetBasketAsync(id);

            return basket is null ? NotFound(new { Message = "Basket not found" }) : Ok(basket);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasketDto>> UpdateCustomerBasket([FromBody] CustomerBasketDto basketDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

             Map DTO to Entity
            var basket = new CustomerBasket
            {
                Id = basketDto.Id,
                PaymentId = basketDto.PaymentIntentId,
                ShippingPrice = basketDto.ShippingPrice,
                ShippingAddress = basketDto.ShippingAddress,
                DeliveryMethodId = basketDto.DeliveryMethodId,
                Items = basketDto.Items.Select(i => new BasketIteam
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    PictureUrl = i.PictureUrl,
                    Brand = i.Brand,
                    Type = i.Type,


                }).ToList()
            };

            var updatedBasket = await _basketRepository.UpdateCustomerBasketAsync(basket);

            return updatedBasket is null
                ? BadRequest(new { Message = "Failed to update basket" })
                : Ok(updatedBasket);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteCustomerBasket(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { Message = "Basket ID is required" });

            var result = await _basketRepository.DeleteCustomerBasketAsync(id);

            return result ? Ok(new { Success = true }) : NotFound(new { Message = "Basket not found" });
        }
    }
}
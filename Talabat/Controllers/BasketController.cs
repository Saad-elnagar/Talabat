using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Repository.Repository;

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
        public async Task<ActionResult<CustomarBasket>> GetCutomarBasket(string id)
        {

            var basket = await _basketRepository.GetCustomerAsync(id);

            return basket is null ? NotFound() : Ok(basket);

        }

        [HttpPost]
        public async Task<ActionResult<CustomarBasket>> UpdateCustomarBasket(CustomarBasket basket)
        {
            var basket_CreateOrUpdate = await _basketRepository.UpdateCustomerBasketAsync(basket);


            return basket_CreateOrUpdate is null ? NotFound() : Ok(basket_CreateOrUpdate);

        }
        [HttpDelete]

        public async Task<ActionResult<bool>> DeletCustomarBasket(CustomarBasket basket)
        {

            return await _basketRepository.DeleteCustomerBasketAsync(basket.Id);

        }
    }
}

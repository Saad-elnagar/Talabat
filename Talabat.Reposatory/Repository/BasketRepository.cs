using Abp.Domain.Uow;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;

namespace Talabat.Repository.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _database;

        public BasketRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteCustomerBasketAsync(string basketId)
        {
            return await _database.KeyDeleteAsync(basketId);
        }

        public async Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            var basket = await _database.StringGetAsync(basketId);

            if (basket.IsNullOrEmpty)
                return null;
            
            
            return JsonSerializer.Deserialize<CustomerBasket>(basket!);
        }

        public async Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket basket)
        {
           
            var serializedBasket = JsonSerializer.Serialize(basket);

         
            var createdOrUpdated = await _database.StringSetAsync(
                basket.Id,
                serializedBasket,
                TimeSpan.FromDays(5) 
            );

            if (!createdOrUpdated)
                return null;

          
            return await GetBasketAsync(basket.Id);
        }
    }
}
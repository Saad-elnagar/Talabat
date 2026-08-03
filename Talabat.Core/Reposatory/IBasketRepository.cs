using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;

namespace Talabat.Core.Reposatory
{
    public interface IBasketRepository
    {

        public Task<CustomerBasket?> GetBasketAsync(string basketId);

        public Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket basket);

        public Task<bool> DeleteCustomerBasketAsync(string basketId);






    }
}

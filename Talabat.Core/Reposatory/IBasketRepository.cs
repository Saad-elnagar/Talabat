using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;

namespace Talabat.Core.Reposatory
{
    public   interface  IBasketRepository 
    {
       
        Task<CustomerBasket?> GetBasketAsync(string basketId);

        Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket basket);

        Task<bool> DeleteCustomerBasketAsync(string basketId);
         





    }
}

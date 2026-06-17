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
         Task<CustomarBasket?> GetCustomerAsync(string baketid);

        Task<CustomarBasket?> UpdateCustomerBasketAsync(CustomarBasket basket);

        Task<bool> DeleteCustomerBasketAsync(string baketid);
         





    }
}

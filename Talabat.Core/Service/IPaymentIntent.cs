using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;

namespace Talabat.Core.Service
{
    public interface IPaymentIntent
    {
        public Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId);
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities;

namespace Talabat.Domain.Repositories
{
    public interface IPaymentIntent
    {
        public Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Identity;

namespace Talabat.Core.Service
{
    public interface ITokenService
    {

        public Task<string> GenerateTokenAsync(User user);


    }
}

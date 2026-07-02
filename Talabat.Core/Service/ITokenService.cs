using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Identity;
using Talabat.Dto;

namespace Talabat.Core.Service
{
    public interface ITokenService
    {

        public Task<TokenDto> GenerateTokenAsync(User user);
        public Task<TokenDto> RefreshTokenAsync(string token, string refreshToken);



    }
}

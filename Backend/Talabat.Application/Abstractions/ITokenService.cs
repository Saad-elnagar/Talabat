using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Domain.Entities.Identity;
using Talabat.Application.DTOs;

namespace Talabat.Application.Abstractions
{
    public interface ITokenService
    {

        public Task<TokenDto> GenerateTokenAsync(User user);
        public Task<TokenDto> RefreshTokenAsync(string token, string refreshToken);



    }
}

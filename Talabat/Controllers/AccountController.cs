using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entitys.Identity;
using Talabat.Core.Service;  // <-- أضف ده
using Talabat.Dto;
using Talabat.Erorr;

namespace Talabat.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;  // <-- أضف ده

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService)  // <-- أضف ده
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized(new APIRespose(401, "Invalid email or password"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new APIRespose(401, "Invalid email or password"));

            return Ok(await _tokenService.GenerateTokenAsync(user));
        }
        

        [HttpPost("register")]
        public async Task<ActionResult<TokenDto>> Register(RegisterDto registerDto)
        {
            var user = new User
            {
                DisplayName = registerDto.DisplayName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // ← هنا هنرجع الـ Errors الحقيقية
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new APIRespose(400, errors));
            }

            return Ok(await _tokenService.GenerateTokenAsync(user));
        }
    }
}
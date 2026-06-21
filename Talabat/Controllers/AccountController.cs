using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly ITokenService _tokenService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService)
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

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new APIRespose(400, errors));
            }

            return Ok(await _tokenService.GenerateTokenAsync(user));
        }
        //  [Authorize]


        [Authorize( Policy = "JwtOnly" )]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new APIRespose(401, "Email claim not found"));

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Unauthorized(new APIRespose(401, "User not found"));
             

            var token = await _tokenService.GenerateTokenAsync(user);
            return Ok(new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = token.Token
            });
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entitys.Identity;
using Talabat.Dto;
using Talabat.Erorr;

namespace Talabat.Controllers
{
    public class AccountController : BaseController
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {


            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>>Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new APIRespose(401, "Invalid email or password"));
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized(new APIRespose(401, "Invalid email or password"));

            return Ok(new UserDto
            {
                Email = user.Email,
                DisplayName = user.UserName,
                Token = await _userManager.GenerateEmailConfirmationTokenAsync(user)
            });
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

          
            var user = new User
            {
                DisplayName = registerDto.DisplayName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber

            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(new APIRespose(400, "Failed to register user"));
            return Ok(new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = await _userManager.GenerateEmailConfirmationTokenAsync(user)
            });

        }



    }
}

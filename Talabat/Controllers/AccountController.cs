
namespace Talabat.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
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
            var checkusermail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (checkusermail != null)
            {
                return BadRequest(new APIRespose(400, "Email is Already .."));
            }


            var user = new User
            {
                DisplayName = registerDto.DisplayName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                RefreshToken = Guid.NewGuid().ToString(),

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new APIRespose(400, errors));
            }

            return Ok(await _tokenService.GenerateTokenAsync(user));
        }
          


        [Authorize(Policy = "JwtOnly")]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailAsync(ClaimTypes.Email);
            var token = await _tokenService.GenerateTokenAsync(user);
            return Ok(new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email!,
                Token = token.Token
            });
        }


        [Authorize(Policy = "JwtOnly")]
        [HttpGet("adderss")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {

            var user = await _userManager.FindByEmailAsync(ClaimTypes.Email);
            if (user == null)
                return Unauthorized(new APIRespose(401, "User not found"));

            var userWithAddress = await _userManager.Users
           .Include(u => u.address)
           .FirstOrDefaultAsync(u => u.Id == user.Id);
            var address = _mapper.Map<Core.Entitys.Identity.Address, AddressDto> (userWithAddress!.address);

            return Ok(address);

        }

        [Authorize(Policy = "JwtOnly")]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto addressDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new APIRespose(401, "Email claim not found"));

            var user = await _userManager.Users
                .Include(u => u.address)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized(new APIRespose(401, "User not found"));

            if (user.address == null)
                user.address = _mapper.Map<Core.Entitys.Identity.Address>(addressDto);
            else
                _mapper.Map(addressDto, user.address);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new APIRespose(400, "Failed to update address"));

            return Ok(_mapper.Map<AddressDto>(user.address));
        }

    }

    }
using IdentityUser.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;
        private readonly SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _distributedCache;

        public AuthController(UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager,
            SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> signInManager,
            IDistributedCache distributedCache, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _distributedCache = distributedCache;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody][Required] UserCredentials userCredentials)
        {
            var newUser = new Microsoft.AspNetCore.Identity.IdentityUser()
            {
                UserName = userCredentials.Email,
                Email = userCredentials.Email,
                EmailConfirmed = true,
            };

            var userCreationResult = await _userManager.CreateAsync(newUser, userCredentials.Password);

            if (!userCreationResult.Succeeded)
                return BadRequest(new { Errors = userCreationResult.Errors });


            var token = GenerateJwtToken(userCredentials.Email);

            var refreshToken = await GenerateRefreshTokenAsync(userCredentials.Email);

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody][Required] UserCredentials userCredentials)
        {
            var user = await _signInManager.PasswordSignInAsync(userCredentials.Email, userCredentials.Password, false, false);

            if (!user.Succeeded)
                return Unauthorized();

            await _distributedCache.RemoveAsync(userCredentials.Email);

            var token = GenerateJwtToken(userCredentials.Email);

            var refreshToken = await GenerateRefreshTokenAsync(userCredentials.Email);

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        [Authorize]
        [HttpGet("authorized-area")]
        public IActionResult AuthorizedArea()
        {
            return Ok(new { message = "Welcome to authorized area" });
        }

        [Authorize]
        [HttpPost("refresh/{refreshtoken}")]
        public async Task<IActionResult> GetRefreshTokenAsync([FromRoute] string refreshtoken)
        {
            var username =_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            var refreshTokenCache = _distributedCache.GetString(username);

            if(refreshtoken != refreshTokenCache)
                return BadRequest();          
           
            var user = await _userManager.FindByEmailAsync(username);

            if(user is null)
                return BadRequest();

            await _distributedCache.RemoveAsync(username);

            var newToken = GenerateJwtToken(username);

            var newRefreshToken = await GenerateRefreshTokenAsync(username);

            return Ok(new
            {
                newToken,
                newRefreshToken
            });
        }

        private async Task<string> GenerateRefreshTokenAsync(string key)
        {
            var refreshToken = Guid.NewGuid().ToString();
            await _distributedCache.SetStringAsync(key, refreshToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
                
            });

            return refreshToken;
        }

        private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("#GREATSUPERSECRET#"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var securityJwtToken = tokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = tokenHandler.WriteToken(securityJwtToken);

            return jwtToken;
        }
    }
}

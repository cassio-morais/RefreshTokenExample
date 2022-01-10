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

        private readonly IDistributedCache _distributedCache;

        public AuthController(UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager,
            SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> signInManager,
            IDistributedCache distributedCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _distributedCache = distributedCache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody][Required] UserCredentials userCredentials)
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


            var token = GenerateJwtToken(userCredentials);

            var refreshToken = await GenerateRefreshToken(userCredentials.Email);

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody][Required] UserCredentials userCredentials)
        {
            var user = await _signInManager.PasswordSignInAsync(userCredentials.Email, userCredentials.Password, false, false);

            if (!user.Succeeded)
                return Unauthorized();

            var token = GenerateJwtToken(userCredentials);

            var refreshToken = await GenerateRefreshToken(userCredentials.Email);

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

        private async Task<string> GenerateRefreshToken(string key)
        {
            var refreshToken = Guid.NewGuid().ToString();
            await _distributedCache.SetStringAsync(key, refreshToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
            });

            return refreshToken;
        }

        private string GenerateJwtToken(UserCredentials userCredentials)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("#GREATSUPERSECRET#"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Name, userCredentials.Email),
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

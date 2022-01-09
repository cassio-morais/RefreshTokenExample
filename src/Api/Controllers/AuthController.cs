using IdentityUser.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;

namespace IdentityUser.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
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

            await _distributedCache.SetStringAsync(Guid.NewGuid().ToString(), "some value");

            await _signInManager.SignInAsync(newUser, false);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody][Required] UserCredentials userCredentials)
        {
            var user = await _signInManager.PasswordSignInAsync(userCredentials.Email, userCredentials.Password, false, false);

            if (!user.Succeeded)
                return BadRequest();

            return Ok();
        }
    }
}

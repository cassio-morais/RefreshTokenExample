using IdentityUser.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IdentityUser.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        private readonly SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> _signInManager;

        public AuthController(UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager,
            SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                return BadRequest();

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

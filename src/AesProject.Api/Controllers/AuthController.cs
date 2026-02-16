using AesProject.Api.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AesProject.Api.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("onboard")]
        public async Task<IActionResult> Onboard([FromBody] OnboardRequestDto request)
        {
            // Using username as email for simplicity
            var user = new IdentityUser { UserName = request.Username, Email = request.Username };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // Auto sign-in or just return OK? Java code didn't sign in, just created user.
                // But Java user likely had to login.
                // I'll return Ok.
                return Ok();
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Find by email (username)
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null) return Unauthorized();

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Unauthorized();
        }
    }
}

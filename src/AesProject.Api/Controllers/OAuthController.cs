using AesProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AesProject.Api.Controllers
{
    [ApiController]
    [Route("api/v1/oauth2")]
    public class OAuthController : ControllerBase
    {
        private readonly IGoogleDriveService _driveService;

        public OAuthController(IGoogleDriveService driveService)
        {
            _driveService = driveService;
        }

        [Authorize]
        [HttpGet("authorize-drive")]
        public IActionResult AuthorizeDrive()
        {
            var userId = User.Identity?.Name;
            if (userId == null) return Unauthorized();

            if (_driveService.IsDriveAuthorized(userId))
            {
                return Ok("Drive is already authorized.");
            }

            var redirectUri = Url.Action("HandleRedirect", "OAuth", null, Request.Scheme);
            var url = _driveService.GetAuthorizationUrl(redirectUri);
            return Redirect(url);
        }

        [Authorize] // Ensure user is logged in for callback to map token to user
        [HttpGet("handle-redirect")]
        public async Task<IActionResult> HandleRedirect(string code)
        {
            var userId = User.Identity?.Name;
            if (userId == null) return Unauthorized();

            var redirectUri = Url.Action("HandleRedirect", "OAuth", null, Request.Scheme);
            await _driveService.ExchangeCodeForTokenAsync(code, userId, redirectUri);

            return Redirect("/index.html");
        }
    }
}

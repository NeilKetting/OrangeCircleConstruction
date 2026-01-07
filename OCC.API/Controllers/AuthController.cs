using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OCC.API.Services;
using OCC.Shared.Models;
using OCC.Shared.DTOs;
using System.Security.Claims;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, token, user, error) = await _authService.LoginAsync(request);

            if (!success)
            {
                if (error.Contains("pending approval"))
                    return StatusCode(403, error);
                
                return Unauthorized(error);
            }

            return Ok(new { Token = token, User = user });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userId != null)
            {
                await _authService.LogoutAsync(userId);
            }
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var (success, createdUser, error) = await _authService.RegisterAsync(user);

            if (!success)
            {
                return Conflict(error);
            }

            return Ok(createdUser);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var success = await _authService.VerifyEmailAsync(token);

            if (!success)
                return BadRequest("Invalid or expired token.");

            return Content(@"
                <html>
                    <head><title>Email Verified</title></head>
                    <body style='font-family: Arial; text-align: center; padding: 50px;'>
                        <h1 style='color: green;'>Email Verified!</h1>
                        <p>Thank you for verifying your email address.</p>
                        <p>You can now close this window and log in to the application.</p>
                    </body>
                </html>", "text/html");
        }
    }
}

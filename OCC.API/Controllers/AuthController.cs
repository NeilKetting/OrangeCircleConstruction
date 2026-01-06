using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OCC.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OCC.API.Data;
using Microsoft.EntityFrameworkCore;
using OCC.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;
using OCC.API.Hubs;
using OCC.API.Services;
using System.Security.Claims;
using System.Text;
using System.Web; // For URL encoding

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;

        public AuthController(IConfiguration configuration, AppDbContext context, IHubContext<NotificationHub> hubContext, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Invalid client request");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            // Verify Password
            // In a real app, hash password verification here
            if (user == null || user.Password != request.Password) 
            {
                // Log Failed Login
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = user?.Id.ToString() ?? "Unknown",
                    TableName = "Users",
                    RecordId = request.Email,
                    Action = "Login Failed",
                    Timestamp = DateTime.UtcNow,
                    NewValues = $"{{ \"Reason\": \"Invalid credentials\", \"Email\": \"{request.Email}\" }}"
                });
                await _context.SaveChangesAsync();

                return Unauthorized("Invalid credentials.");
            }

            // Verify Approval
            if (!user.IsApproved)
            {
                // Log Blocked Login
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = user.Id.ToString(),
                    TableName = "Users",
                    RecordId = user.Id.ToString(),
                    Action = "Login Blocked",
                    Timestamp = DateTime.UtcNow,
                    NewValues = "{ \"Reason\": \"Account not approved\" }"
                });
                await _context.SaveChangesAsync();

                return StatusCode(403, "Account pending approval. Please wait for an administrator to activate your account.");
            }

            var tokenString = GenerateJwtToken(user);
            
            // Log Login Action
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id.ToString(),
                TableName = "Users",
                RecordId = user.Id.ToString(),
                Action = "Login",
                Timestamp = DateTime.UtcNow,
                NewValues = "{ \"Action\": \"User Logged In\" }"
            });
            await _context.SaveChangesAsync();

            return Ok(new { Token = tokenString, User = user });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return Conflict("User already exists");
            }

            // Set default values for new registration
            user.IsApproved = false;
            user.IsEmailVerified = false; // In future, send email verification link here

            // In real app, hash password here
             _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Notify Admins
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New user registered: {user.FirstName} {user.LastName} ({user.Email}) is waiting for approval.");

            // Generate Verification Token
            var verificationToken = GenerateJwtToken(user, 1); // 1 day expiration for verification
            var encodedToken = HttpUtility.UrlEncode(verificationToken);
            var verifyLink = $"https://localhost:7166/api/Auth/verify?token={encodedToken}";

            // Send Verification Email
            var emailBody = $@"
                <p>Hi {user.FirstName},</p>
                <p>Welcome to Orange Circle Construction! Please verify your email address to complete your registration.</p>
                <a href='{verifyLink}' class='button'>Verify Email</a>
                <p>If the button doesn't work, copy and paste this link:</p>
                <p>{verifyLink}</p>
            ";

            await _emailService.SendEmailAsync(user.Email, "Verify Your Email Address", emailBody);

            return Ok(user);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid token");

            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            try
            {
                var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.Name); // We stored ID in Name claim
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return BadRequest("Invalid token content");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                user.IsEmailVerified = true;
                await _context.SaveChangesAsync();

                // Log Verification
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = user.Id.ToString(),
                    TableName = "Users",
                    RecordId = user.Id.ToString(),
                    Action = "Email Verified",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // Return nice HTML page
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
            catch (Exception)
            {
                return BadRequest("Invalid or expired token.");
            }
        }

        private string GenerateJwtToken(User user, int days = 7)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(days),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

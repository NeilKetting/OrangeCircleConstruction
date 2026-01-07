using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using OCC.API.Services;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly ILogger<UsersController> _logger;
        private readonly IHubContext<Hubs.NotificationHub> _hubContext;

        public UsersController(AppDbContext context, PasswordHasher passwordHasher, ILogger<UsersController> logger, Microsoft.AspNetCore.SignalR.IHubContext<OCC.API.Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _hubContext = hubContext;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return Conflict("User with this email already exists.");
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = _passwordHasher.HashPassword(user.Password);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Notify clients
            await _hubContext.Clients.All.SendAsync("EntityUpdate", "User", "Create", user.Id);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New user created: {user.FirstName} {user.LastName}");

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            // Prevent changing own role or locking oneself out ideally, but simple for now
            // If password provided, hash it
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            
            // Note: AsNoTracking means the entity is not tracked. 
            // We need to attach the new 'user' or update properties.
            // Since we set State = Modified below, that attaches it.
            
            if (existingUser != null && user.Password != existingUser.Password) 
            {
                if (!string.IsNullOrEmpty(user.Password))
                {
                   user.Password = _passwordHasher.HashPassword(user.Password);
                }
                else 
                {
                    user.Password = existingUser.Password; // Keep old if empty
                }
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "User", "Update", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "User", "Delete", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

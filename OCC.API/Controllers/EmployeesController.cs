using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using OCC.API.Data;
using OCC.Shared.Models;
using OCC.API.Hubs;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Allow any authenticated user to READ (Get)
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger, IHubContext<Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            try
            {
                return await _context.Employees.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(Guid id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                {
                    return NotFound();
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Employees
        [HttpPost]
        [Authorize(Roles = "Admin")] // Strict: Admin Only
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "Employee", "Create", employee.Id);

                return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Employees/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Strict: Admin Only
        public async Task<IActionResult> PutEmployee(Guid id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "Employee", "Update", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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
                _logger.LogError(ex, "Error updating employee {Id}", id);
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Strict: Admin Only
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                // Safe Deletion Checks
                
                // 1. Task Assignments
                if (await _context.TaskAssignments.AnyAsync(ta => ta.AssigneeId == id))
                {
                    return Conflict("Cannot delete employee: They are assigned to active tasks.");
                }

                // 2. Project Site Manager
                if (await _context.Projects.AnyAsync(p => p.SiteManagerId == id))
                {
                    return Conflict("Cannot delete employee: They are listed as Site Manager on a project.");
                }

                // 3. Team Membership
                if (await _context.TeamMembers.AnyAsync(tm => tm.EmployeeId == id))
                {
                    return Conflict("Cannot delete employee: They are currently a member of a team.");
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "Employee", "Delete", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}

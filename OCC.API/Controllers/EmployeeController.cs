using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.Shared.Models;
using System.Security.Claims;
using System.Text.Json;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetStaffMembers()
        {
            return await _context.StaffMembers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(Guid id)
        {
            var employee = await _context.StaffMembers.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.StaffMembers.Add(employee);

            // Audit Create
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
                TableName = "StaffMembers",
                RecordId = employee.Id.ToString(),
                Action = "Create",
                NewValues = JsonSerializer.Serialize(employee),
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(Guid id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            // Audit Update
            // Fetch existing entity to log changes (AsNoTracking to avoid conflict)
            var existingEmployee = await _context.StaffMembers.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existingEmployee != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
                    TableName = "StaffMembers",
                    RecordId = id.ToString(),
                    Action = "Update",
                    OldValues = JsonSerializer.Serialize(existingEmployee),
                    NewValues = JsonSerializer.Serialize(employee),
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var employee = await _context.StaffMembers.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Audit Delete
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
                TableName = "StaffMembers",
                RecordId = id.ToString(),
                Action = "Delete",
                OldValues = JsonSerializer.Serialize(employee),
                Timestamp = DateTime.UtcNow
            });

            _context.StaffMembers.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(Guid id)
        {
            return _context.StaffMembers.Any(e => e.Id == id);
        }
    }
}

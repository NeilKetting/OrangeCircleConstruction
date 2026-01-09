using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OCC.API.Data;
using OCC.API.Hubs;
using OCC.Shared.Models;

namespace OCC.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceRecordsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<AttendanceRecordsController> _logger;

        public AttendanceRecordsController(AppDbContext context, IHubContext<NotificationHub> hubContext, ILogger<AttendanceRecordsController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: api/AttendanceRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetAttendanceRecords()
        {
            try
            {
                return await _context.AttendanceRecords.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance records");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/AttendanceRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceRecord>> GetAttendanceRecord(Guid id)
        {
            try
            {
                var record = await _context.AttendanceRecords.FindAsync(id);
                if (record == null) return NotFound();
                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance record {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/AttendanceRecords
        [HttpPost]
        public async Task<ActionResult<AttendanceRecord>> PostAttendanceRecord(AttendanceRecord record)
        {
            try
            {
                if (record.Id == Guid.Empty) record.Id = Guid.NewGuid();
                _context.AttendanceRecords.Add(record);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "AttendanceRecord", "Create", record.Id);
                
                return CreatedAtAction("GetAttendanceRecord", new { id = record.Id }, record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attendance record");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/AttendanceRecords/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAttendanceRecord(Guid id, AttendanceRecord record)
        {
            if (id != record.Id) return BadRequest();
            _context.Entry(record).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "AttendanceRecord", "Update", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceRecordExists(id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance record {Id}", id);
                return StatusCode(500, "Internal server error");
            }
            return NoContent();
        }

        // DELETE: api/AttendanceRecords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceRecord(Guid id)
        {
            try
            {
                var record = await _context.AttendanceRecords.FindAsync(id);
                if (record == null) return NotFound();
                _context.AttendanceRecords.Remove(record);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.All.SendAsync("EntityUpdate", "AttendanceRecord", "Delete", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance record {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool AttendanceRecordExists(Guid id) => _context.AttendanceRecords.Any(e => e.Id == id);

        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadNote(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "notes");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            return Ok($"/uploads/notes/{uniqueFileName}");
        }
    }
}

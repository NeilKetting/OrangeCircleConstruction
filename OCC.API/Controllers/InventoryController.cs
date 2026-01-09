using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using OCC.API.Data;
using OCC.API.Hubs;
using OCC.Shared.Models;
using System.Security.Claims;

namespace OCC.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InventoryController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public InventoryController(AppDbContext context, ILogger<InventoryController> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<InventoryItem>>> GetInventory()
        {
            try
            {
                var items = await _context.InventoryItems
                    .OrderBy(i => i.ProductName)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching inventory.");
                return StatusCode(500, "An error occurred while fetching inventory.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItem(Guid id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);

                if (item == null)
                    return NotFound();

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching inventory item {ItemId}", id);
                return StatusCode(500, "An error occurred while fetching the inventory item.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItem>> CreateItem(InventoryItem item)
        {
             try
            {
                if (item == null) return BadRequest("Item data is null.");

                item.Id = Guid.NewGuid();
                
                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inventory item {ProductName} created by {User}", item.ProductName, User.FindFirst(ClaimTypes.Name)?.Value);

                // Notify clients
                await _hubContext.Clients.All.SendAsync("ReceiveInventoryUpdate", "ItemCreated");

                return CreatedAtAction(nameof(GetInventoryItem), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating inventory item {ProductName}", item?.ProductName);
                return StatusCode(500, "An error occurred while creating the inventory item.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, InventoryItem item)
        {
            if (id != item.Id)
                return BadRequest();

            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Inventory item {ProductName} updated by {User}", item.ProductName, User.FindFirst(ClaimTypes.Name)?.Value);

                // Notify clients
                 await _hubContext.Clients.All.SendAsync("ReceiveInventoryUpdate", "ItemUpdated");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating inventory item {ItemId}", id);
                return StatusCode(500, "An error occurred while updating the inventory item.");
            }
        }

        private bool InventoryItemExists(Guid id)
        {
            return _context.InventoryItems.Any(e => e.Id == id);
        }
    }
}

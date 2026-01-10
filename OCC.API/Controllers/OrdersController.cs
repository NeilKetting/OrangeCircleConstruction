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
    [Authorize(Roles = "Admin,Office")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrdersController(AppDbContext context, ILogger<OrdersController> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Lines)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching orders.");
                return StatusCode(500, "An error occurred while fetching orders.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Lines)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching order {OrderId}", id);
                return StatusCode(500, "An error occurred while fetching the order.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            try
            {
                if (order == null) return BadRequest("Order data is null.");

                // Validate order
                if (!order.Lines.Any())
                    return BadRequest("Order must have at least one line item.");

                // Set server-side properties (safety)
                order.Id = Guid.NewGuid();
                order.OrderDate = DateTime.UtcNow; // Or keep client date if intended, but safety dictates UTC server time usually

                // Ensure lines have IDs
                foreach (var line in order.Lines)
                {
                    line.Id = Guid.NewGuid();
                    line.OrderId = order.Id;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} created by {User}", order.OrderNumber, User.FindFirst(ClaimTypes.Name)?.Value);

                // Notify clients via SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", order);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order {OrderNumber}", order?.OrderNumber);
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, Order order)
        {
            if (id != order.Id)
                return BadRequest();

            try
            {
                _context.Entry(order).State = EntityState.Modified;

                // Handle lines (simple approach: remove all and re-add for prototype simplicity, 
                // typically we'd reconcile)
                // For MVP/Robust persistence logic, we need to handle child lines carefully.
                // 1. Load existing
                var existingOrder = await _context.Orders
                                            .Include(o => o.Lines)
                                            .FirstOrDefaultAsync(o => o.Id == id);
                                            
                if (existingOrder == null) return NotFound();

                var oldStatus = existingOrder.Status;
                
                // 2. Update scalar properties
                _context.Entry(existingOrder).CurrentValues.SetValues(order);

                // 3. Reconcile Lines (Smart Merge)
                foreach (var line in order.Lines)
                {
                    var existingLine = existingOrder.Lines.FirstOrDefault(l => l.Id == line.Id);
                    if (existingLine != null)
                    {
                        // Update existing
                        _context.Entry(existingLine).CurrentValues.SetValues(line);
                    }
                    else
                    {
                        // Add new
                        line.OrderId = order.Id;
                        _context.OrderLines.Add(line);
                    }
                }

                // Remove deleted lines
                var linesToRemove = existingOrder.Lines
                    .Where(l => !order.Lines.Any(ol => ol.Id == l.Id))
                    .ToList();

                if (linesToRemove.Any())
                {
                    _context.OrderLines.RemoveRange(linesToRemove);
                }

                // 4. Inventory Logic: If Status changes to Completed (or specific trigger)
                // We assume once Completed, stock is moved. 
                // Note: NOT handling reversion if status moves BACK from Completed.
                // 4. Inventory Logic
                // REMOVED: Managed by granular ReceiveOrder logic in Client/Service to support partial deliveries.
                // Previous logic only handled "On Complete" and risked double-counting or missing partial updates.
                
                /* 
                if (oldStatus != OrderStatus.Completed && order.Status == OrderStatus.Completed)
                {
                    // Legacy auto-update removed
                } 
                */

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} updated by {User}", order.OrderNumber, User.FindFirst(ClaimTypes.Name)?.Value);

                // Notify clients
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", order);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    return NotFound();

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} deleted by {User}", order.OrderNumber, User.FindFirst(ClaimTypes.Name)?.Value);

                // Notify clients
                await _hubContext.Clients.All.SendAsync("ReceiveOrderDelete", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order {OrderId}", id);
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }
    }
}

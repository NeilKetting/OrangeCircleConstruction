using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync();
        Task<Order?> GetOrderAsync(Guid id);
        Task<Order> CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Guid id);
    }
}

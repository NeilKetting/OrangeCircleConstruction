using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotificationsAsync();
        Task MarkAsReadAsync(Guid notificationId);
        Task ClearAllAsync();
        
        // Helper to send a simplified notification
        Task SendReminderAsync(string title, string message, string? action = null);
        
        event EventHandler<Notification>? NotificationReceived;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockNotificationService : INotificationService
    {
        private readonly List<Notification> _notifications = new();

        public event EventHandler<Notification>? NotificationReceived;

        public MockNotificationService()
        {
            // Initial mock notification
            _notifications.Add(new Notification 
            { 
                Title = "Roll-Call Reminder", 
                Message = "Daily roll-call has not been performed for Site: Cape Town House.",
                Timestamp = DateTime.Today.AddHours(8)
            });
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync()
        {
            return await Task.FromResult(_notifications.OrderByDescending(n => n.Timestamp));
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var note = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (note != null) note.IsRead = true;
            await Task.CompletedTask;
        }

        public async Task ClearAllAsync()
        {
            _notifications.Clear();
            await Task.CompletedTask;
        }

        public async Task SendReminderAsync(string title, string message, string? action = null)
        {
            var note = new Notification
            {
                Title = title,
                Message = message,
                TargetAction = action,
                Timestamp = DateTime.Now
            };
            _notifications.Add(note);
            NotificationReceived?.Invoke(this, note);
            await Task.CompletedTask;
        }
    }
}

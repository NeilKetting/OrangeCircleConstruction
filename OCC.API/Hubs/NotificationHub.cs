using Microsoft.AspNetCore.SignalR;

namespace OCC.API.Hubs
{
    public class NotificationHub : Hub
    {
        // Track connected users: ConnectionId -> UserConnectionInfo
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, OCC.Shared.DTOs.UserConnectionInfo> _connectedUsers 
            = new System.Collections.Concurrent.ConcurrentDictionary<string, OCC.Shared.DTOs.UserConnectionInfo>();

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userName = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value 
                               ?? Context.User?.Identity?.Name 
                               ?? "Anonymous";
                var id = Context.ConnectionId;
                
                var info = new OCC.Shared.DTOs.UserConnectionInfo 
                { 
                    UserName = userName, 
                    ConnectedAt = DateTime.UtcNow 
                };

                _connectedUsers.TryAdd(id, info);
                await BroadcastUserList();
            }
            catch (Exception ex)
            {
                // Log?
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var id = Context.ConnectionId;
                _connectedUsers.TryRemove(id, out _);
                await BroadcastUserList();
            }
            catch { }
            
            await base.OnDisconnectedAsync(exception);
        }

        private async Task BroadcastUserList()
        {
            // Distinct users by name, taking the earliest connection time
            var users = _connectedUsers.Values
                .GroupBy(u => u.UserName)
                .Select(g => g.OrderBy(u => u.ConnectedAt).First())
                .OrderBy(u => u.UserName)
                .ToList();
                
            await Clients.All.SendAsync("UserListUpdate", users);
        }

        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}

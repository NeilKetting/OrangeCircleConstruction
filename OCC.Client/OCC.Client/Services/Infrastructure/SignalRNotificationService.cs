using Microsoft.AspNetCore.SignalR.Client;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace OCC.Client.Services.Infrastructure
{
    public class SignalRNotificationService : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        public event Action<string>? OnNotificationReceived;

        public SignalRNotificationService()
        {
            var baseUrl = ConnectionSettings.Instance.ApiBaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            
            // Adjust protocol if needed (http vs https). 
            // The API is http://102.39.20.146:8081/, so SignalR is on the same host/port.
            // CAREFUL: SignalR client might default to forcing HTTPS or need options.
            // If the server is HTTP, we use HTTP.
            
            var hubUrl = $"{baseUrl}hubs/notifications";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl) 
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("ReceiveNotification", (message) =>
            {
                OnNotificationReceived?.Invoke(message);
            });

            _hubConnection.On<string, string, Guid>("EntityUpdate", (entityType, action, id) =>
            {
                // Dispatch to UI thread if needed, but Messenger handles logic. 
                // Using WeakReferenceMessenger to broadcast
                var msg = new ViewModels.Messages.EntityUpdatedMessage(entityType, action, id);
                WeakReferenceMessenger.Default.Send(msg);
            });

             _hubConnection.On<System.Collections.Generic.List<OCC.Shared.DTOs.UserConnectionInfo>>("UserListUpdate", (users) =>
            {
                OnUserListReceived?.Invoke(users);
            });
        }
        
        public event Action<System.Collections.Generic.List<OCC.Shared.DTOs.UserConnectionInfo>>? OnUserListReceived;


        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // Handle connection errors (log them)
                System.Diagnostics.Debug.WriteLine($"SignalR Connection Failed: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}

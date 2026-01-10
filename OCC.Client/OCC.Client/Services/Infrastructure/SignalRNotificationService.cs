using Microsoft.AspNetCore.SignalR.Client;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace OCC.Client.Services.Infrastructure
{
    public class SignalRNotificationService : IAsyncDisposable
    {
        private HubConnection _hubConnection = null!;

        public event Action<string>? OnNotificationReceived;

        private readonly IServiceProvider _serviceProvider;
        private readonly Services.Interfaces.IAuthService _authService;

        public SignalRNotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _authService = (Services.Interfaces.IAuthService)_serviceProvider.GetService(typeof(Services.Interfaces.IAuthService))!; // Service locator to avoid circle if any (though AuthService shouldn't depend on SignalR)
            
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            var baseUrl = ConnectionSettings.Instance.ApiBaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            
            var hubUrl = $"{baseUrl}hubs/notifications";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_authService?.AuthToken);
                }) 
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("ReceiveNotification", (message) =>
            {
                OnNotificationReceived?.Invoke(message);
            });

            _hubConnection.On<string, string, Guid>("EntityUpdate", (entityType, action, id) =>
            {
                var msg = new ViewModels.Messages.EntityUpdatedMessage(entityType, action, id);
                WeakReferenceMessenger.Default.Send(msg);
            });

            // Specific Listeners for Orders
            _hubConnection.On<OCC.Shared.Models.Order>("ReceiveOrderUpdate", (order) =>
            {
                 // Broadcast as generic Entity Update for simplicity
                 var msg = new ViewModels.Messages.EntityUpdatedMessage("Order", "Update", order.Id);
                 WeakReferenceMessenger.Default.Send(msg);
            });

            _hubConnection.On<Guid>("ReceiveOrderDelete", (id) =>
            {
                 var msg = new ViewModels.Messages.EntityUpdatedMessage("Order", "Delete", id);
                 WeakReferenceMessenger.Default.Send(msg);
            });

             _hubConnection.On<System.Collections.Generic.List<OCC.Shared.DTOs.UserConnectionInfo>>("UserListUpdate", (users) =>
            {
                OnUserListReceived?.Invoke(users);
            });
        }

        public async Task RestartAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            InitializeConnection();
            await StartAsync();
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

        public async Task UpdateStatusAsync(string status)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("UpdateStatus", status);
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

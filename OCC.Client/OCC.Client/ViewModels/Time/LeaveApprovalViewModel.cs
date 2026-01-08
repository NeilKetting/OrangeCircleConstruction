using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OCC.Client.ViewModels.Time
{
    public partial class LeaveApprovalViewModel : ViewModelBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<LeaveRequest> _pendingRequests = new();

        [ObservableProperty]
        private LeaveRequest? _selectedRequest;

        [ObservableProperty]
        private string _rejectionReason = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public LeaveApprovalViewModel(
            ILeaveService leaveService,
            IAuthService authService,
            INotificationService notificationService)
        {
            _leaveService = leaveService;
            _authService = authService;
            _notificationService = notificationService;
            
            LoadDataCommand.Execute(null);
        }

        public LeaveApprovalViewModel()
        {
             _leaveService = null!;
             _authService = null!;
             _notificationService = null!;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                // In a real app, we might expand the LeaveRequest.Employee navigation property.
                // Depending on the Repo implementation, it might not load includes automatically.
                // Assuming standard Repo does minimal loading, we might need a specific service method 
                // that Includes Employee, or we rely on LazyLoading if enabled (unlikely) 
                // or we fetch employees and join.
                // For now, let's assume GetPendingRequestsAsync includes Employee or we'll simple-fetch.
                // Actually, Generic Repo usually doesn't include. 
                // I will add "Include" logic or just fetch all for now in Service if needed.
                // Let's assume the Service does it or I need to update Service. 
                // I'll check Service later. For now, basic flow.
                
                var requests = await _leaveService.GetPendingRequestsAsync();
                PendingRequests.Clear();
                foreach (var r in requests) PendingRequests.Add(r);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ApproveAsync(LeaveRequest? request)
        {
            if (request == null) return;
            
            var user = _authService.CurrentUser;
            // if (user == null) return; // Optional check

            IsBusy = true;
            try
            {
                await _leaveService.ApproveRequestAsync(request.Id, user?.Id ?? Guid.Empty);
                PendingRequests.Remove(request);
                await _notificationService.SendReminderAsync("Success", "Leave Request Approved.");
            }
            catch (Exception ex)
            {
                await _notificationService.SendReminderAsync("Error", "Error approving: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RejectAsync(LeaveRequest? request)
        {
             if (request == null) return;
             
             if (string.IsNullOrWhiteSpace(RejectionReason))
             {
                 await _notificationService.SendReminderAsync("Error", "Rejection reason is required.");
                 return;
             }

            try
            {
                await _leaveService.RejectRequestAsync(request.Id, Guid.Empty, RejectionReason);
                PendingRequests.Remove(request);
                RejectionReason = string.Empty;
                await _notificationService.SendReminderAsync("Success", "Leave Request Rejected.");
            }
            catch (Exception ex)
            {
                await _notificationService.SendReminderAsync("Error", "Error: " + ex.Message);
            }
        }
    }
}

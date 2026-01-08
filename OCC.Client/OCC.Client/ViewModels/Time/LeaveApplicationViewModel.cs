using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class LeaveApplicationViewModel : ViewModelBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        private DateTimeOffset? _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTimeOffset? _endDate = DateTime.Today;

        [ObservableProperty]
        private LeaveType _selectedLeaveType = LeaveType.Annual;
        
        public ObservableCollection<LeaveType> LeaveTypes { get; } = new(Enum.GetValues<LeaveType>());

        [ObservableProperty]
        private string _reason = string.Empty;

        [ObservableProperty]
        private int _calculatedDays;

        [ObservableProperty]
        private bool _isCalculated;

        [ObservableProperty]
        private string _balanceWarning = string.Empty;

        [ObservableProperty]
        private bool _hasBalanceWarning;

        [ObservableProperty]
        private bool _isSubmitting;

        public LeaveApplicationViewModel(
            ILeaveService leaveService,
            IRepository<Employee> employeeRepository,
            INotificationService notificationService)
        {
            _leaveService = leaveService;
            _employeeRepository = employeeRepository;
            _notificationService = notificationService;
            
            LoadDataCommand.Execute(null);
        }

        // Parameterless constructor for designer
        public LeaveApplicationViewModel() 
        {
             _leaveService = null!;
             _employeeRepository = null!;
             _notificationService = null!;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
             var emps = await _employeeRepository.GetAllAsync();
             Employees.Clear();
             foreach(var e in emps.OrderBy(e => e.LastName)) Employees.Add(e);
             
             if (Employees.Any()) SelectedEmployee = Employees.First();
        }

        partial void OnStartDateChanged(DateTimeOffset? value) => RecalculateDaysCommand.Execute(null);
        partial void OnEndDateChanged(DateTimeOffset? value) => RecalculateDaysCommand.Execute(null);
        partial void OnSelectedEmployeeChanged(Employee? value) => RecalculateDaysCommand.Execute(null);
        partial void OnSelectedLeaveTypeChanged(LeaveType value) => RecalculateDaysCommand.Execute(null);

        [RelayCommand]
        private async Task RecalculateDaysAsync()
        {
            if (StartDate == null || EndDate == null) 
            {
                CalculatedDays = 0;
                IsCalculated = false;
                HasBalanceWarning = false;
                return;
            }

            var start = StartDate.Value.Date;
            var end = EndDate.Value.Date;

            CalculatedDays = await _leaveService.CalculateBusinessDaysAsync(start, end);
            IsCalculated = true;
            
            CheckBalance();
        }

        private void CheckBalance()
        {
            HasBalanceWarning = false;
            BalanceWarning = string.Empty;

            if (SelectedEmployee == null) return;
            if (SelectedLeaveType != LeaveType.Annual) return; // Only check Annual Leave balance for now

            if (CalculatedDays > SelectedEmployee.LeaveBalance)
            {
                double shortfall = CalculatedDays - SelectedEmployee.LeaveBalance;
                HasBalanceWarning = true;
                BalanceWarning = $"Insufficient Leave Balance ({SelectedEmployee.LeaveBalance} days available). {shortfall} days will be UNPAID.";
            }
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (SelectedEmployee == null)
            {
                await _notificationService.SendReminderAsync("Error", "Select an employee.");
                return;
            }
            if (SelectedLeaveType == LeaveType.Annual && SelectedEmployee.LeaveBalance < 0) 
            {
                // Balance warning already shown, but maybe block? User didn't say block.
            }
            
            if (StartDate == null || EndDate == null)
            {
                 await _notificationService.SendReminderAsync("Error", "Select both start and end dates.");
                 return;
            }
            
            if (EndDate < StartDate)
            {
                await _notificationService.SendReminderAsync("Error", "End date must be after start date.");
                return;
            }

            IsSubmitting = true;
            try
            {
                await _leaveService.SubmitRequestAsync(new LeaveRequest
                {
                    EmployeeId = SelectedEmployee.Id,
                    StartDate = StartDate.Value.Date,
                    EndDate = EndDate.Value.Date,
                    LeaveType = SelectedLeaveType,
                    Reason = Reason,
                    IsUnpaid = HasBalanceWarning // Assuming IsUnpaid is determined by HasBalanceWarning
                });

                await _notificationService.SendReminderAsync("Success", "Leave Request Submitted Successfully.");
                
                // Reset form
                Reason = string.Empty;
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                await _notificationService.SendReminderAsync("Error", $"Failed to submit request: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}

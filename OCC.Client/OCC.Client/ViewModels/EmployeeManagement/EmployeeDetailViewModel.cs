using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Employee> _staffRepository;
        private readonly IDialogService _dialogService;
        private Guid? _existingStaffId;
        private DateTime _calculatedDoB = DateTime.Now.AddYears(-30);

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? EmployeeAdded;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _employeeNumber = string.Empty;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _idNumber = string.Empty;

        [ObservableProperty]
        private string _permitNumber = string.Empty;

        [ObservableProperty]
        private IdType _selectedIdType = IdType.RSAId;

        [ObservableProperty]
        private string _phone = string.Empty; 

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private EmployeeRole _selectedSkill = EmployeeRole.GeneralWorker;

        [ObservableProperty]
        private double _hourlyRate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmploymentDateDateTime))]
        private DateTimeOffset _employmentDate = DateTimeOffset.Now;

        public DateTime EmploymentDateDateTime
        {
            get => EmploymentDate.DateTime;
            set => EmploymentDate = value;
        }

        [ObservableProperty]
        private EmploymentType _selectedEmploymentType = EmploymentType.Permanent;

        [ObservableProperty]
        private string _contractDuration = string.Empty;

        [ObservableProperty]
        private string _title = "Add Employee";

        [ObservableProperty]
        private string _saveButtonText = "Add Employee";

        [ObservableProperty]
        private string _branch = "Johannesburg";

        [ObservableProperty]
        private TimeSpan? _shiftStartTime = new TimeSpan(7, 0, 0);

        [ObservableProperty]
        private TimeSpan? _shiftEndTime = new TimeSpan(16, 45, 0);

        // Banking Details


        [ObservableProperty]
        private string _accountNumber = string.Empty;

        [ObservableProperty]
        private string _taxNumber = string.Empty;

        [ObservableProperty]
        private string _branchCode = string.Empty;

        [ObservableProperty]
        private string _accountType = "Select Account Type";

        public List<string> AccountTypes { get; } = new() { "Select Account Type", "Savings", "Cheque", "Transmission" };



        [ObservableProperty]
        private RateType _selectedRateType = RateType.Hourly;

        // Leave Balances
        [ObservableProperty]
        private double _annualLeaveBalance;

        [ObservableProperty]
        private double _sickLeaveBalance = 30; // Default SA Limit

        [ObservableProperty]
        private DateTimeOffset? _leaveCycleStartDate;

        [ObservableProperty]
        private string _sickLeaveCycleEndDisplay = "N/A";

        [ObservableProperty]
        private string _leaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";

        #endregion

        #region Properties

        public bool IsHourly
        {
            get => SelectedRateType == RateType.Hourly;
            set
            {
                if (value) SelectedRateType = RateType.Hourly;
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
            }
        }

        public bool IsSalary
        {
            get => SelectedRateType == RateType.MonthlySalary;
            set
            {
                if (value) SelectedRateType = RateType.MonthlySalary;
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
            }
        }
        
        public bool IsRsaId
        {
            get => SelectedIdType == IdType.RSAId;
            set
            {
                if (value) SelectedIdType = IdType.RSAId;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPassport
        {
            get => SelectedIdType == IdType.Passport;
            set
            {
                if (value) SelectedIdType = IdType.Passport;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPermanent
        {
            get => SelectedEmploymentType == EmploymentType.Permanent;
            set
            {
                if (value) SelectedEmploymentType = EmploymentType.Permanent;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContract
        {
            get => SelectedEmploymentType == EmploymentType.Contract;
            set
            {
                if (value) SelectedEmploymentType = EmploymentType.Contract;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContractVisible => IsContract;

        // Exposed Enum Values for ComboBox
        public EmployeeRole[] EmployeeRoles { get; } = Enum.GetValues<EmployeeRole>();

        public List<string> Branches { get; } = new() { "Johannesburg", "Cape Town" };

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
                {
                    return _existingStaffId.HasValue ? "Edit Employee" : "New Employee";
                }
                return $"{FirstName}, {LastName}".Trim();
            }
        }

        #endregion

        #region Constructors

        public EmployeeDetailViewModel(IRepository<Employee> staffRepository, IDialogService dialogService)
        {
            _staffRepository = staffRepository;
            _dialogService = dialogService;
        }

        public EmployeeDetailViewModel() 
        {
            // _staffRepository and _dialogService will be null, handle in Save
            _staffRepository = null!;
            _dialogService = null!;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Save()
        {
            // 1. Mandatory Fields Validation
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                await _dialogService.ShowAlertAsync("Validation Error", "First Name is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(LastName))
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Last Name is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(IdNumber) && IsRsaId)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "ID Number is required.");
                return;
            }

            // 2. Duplicate Checks
            // We need to fetch existing employees to check for duplicates.
            // Ideally Repository has unique check methods, but we'll fetch all or use Find.
            // _staffRepository.GetAllAsync() might be heavy if lots of users, but sufficient for now.
            var allStaff = await _staffRepository.GetAllAsync();
            
            // Filter out current user if editing
            var otherStaff = _existingStaffId.HasValue 
                ? allStaff.Where(s => s.Id != _existingStaffId.Value).ToList() 
                : allStaff.ToList();

            // Check ID Number
            if (!string.IsNullOrWhiteSpace(IdNumber) && otherStaff.Any(s => s.IdNumber == IdNumber))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with ID Number '{IdNumber}' already exists.");
                 return;
            }

            // Check Employee Number
            if (!string.IsNullOrWhiteSpace(EmployeeNumber) && otherStaff.Any(s => s.EmployeeNumber == EmployeeNumber))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with Number '{EmployeeNumber}' already exists.");
                 return;
            }

            // Check Email
            if (!string.IsNullOrWhiteSpace(Email) && otherStaff.Any(s => s.Email != null && s.Email.Equals(Email, StringComparison.OrdinalIgnoreCase)))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with Email '{Email}' already exists.");
                 return;
            }

            Employee staff;

            if (_existingStaffId.HasValue)
            {
                // Update existing
                staff = await _staffRepository.GetByIdAsync(_existingStaffId.Value) ?? new Employee { Id = _existingStaffId.Value };
            }
            else
            {
                // Create new
                staff = new Employee();
            }

            // Map properties
            staff.EmployeeNumber = EmployeeNumber;
            staff.FirstName = FirstName;
            staff.LastName = LastName;
            staff.IdNumber = IdNumber;
            staff.PermitNumber = PermitNumber;
            staff.IdType = SelectedIdType;
            staff.Email = Email;
            staff.Phone = Phone;
            staff.Role = SelectedSkill;
            staff.HourlyRate = HourlyRate;
            staff.EmploymentType = SelectedEmploymentType;
            staff.DoB = _calculatedDoB;
            staff.EmploymentDate = EmploymentDate.DateTime;
            staff.ContractDuration = ContractDuration;
            staff.Branch = Branch;
            staff.ShiftStartTime = ShiftStartTime;
            staff.Branch = Branch; // Redundant line in original, kept to match structure or remove
            staff.ShiftStartTime = ShiftStartTime; // Redundant
            staff.ShiftEndTime = ShiftEndTime;
            // Removed redundant assignments for cleanliness
            
            // Leave Balances
            staff.AnnualLeaveBalance = AnnualLeaveBalance;
            staff.SickLeaveBalance = SickLeaveBalance;
            staff.LeaveCycleStartDate = LeaveCycleStartDate?.DateTime;
            
            // Banking
            // Map "Select Bank" (None) to null/empty
            if (SelectedBank == OCC.Shared.Models.BankName.None)
            {
                 staff.BankName = null;
            }
            else
            {
                 staff.BankName = IsOtherBankSelected ? CustomBankName : GetEnumDescription(SelectedBank);
            }

            staff.AccountNumber = AccountNumber;
            staff.TaxNumber = TaxNumber;
            staff.BranchCode = BranchCode;
            
            // Map "Select Account Type" to null
            staff.AccountType = (AccountType == "Select Account Type") ? null : AccountType;
            
            staff.RateType = SelectedRateType;

            if (_staffRepository != null)
            {
                try 
                {
                    if (_existingStaffId.HasValue)
                    {
                        await _staffRepository.UpdateAsync(staff);
                    }
                    else
                    {
                        await _staffRepository.AddAsync(staff);
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Failed to save employee: {ex.Message}");
                    return;
                }
            }
            
            EmployeeAdded?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void SetSelectedBank(BankName bank)
        {
            SelectedBank = bank;
        }

        #endregion

        #region Methods

        public void Load(Employee staff)
        {
            if (staff == null) return;

            try 
            {
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Loading Employee: {staff.Id}");

                _existingStaffId = staff.Id;
                Title = "Edit Employee";
                SaveButtonText = "Save Changes";

                EmployeeNumber = staff.EmployeeNumber;
                FirstName = staff.FirstName;
                LastName = staff.LastName;
                IdNumber = staff.IdNumber;
                PermitNumber = staff.PermitNumber ?? string.Empty;
                SelectedIdType = staff.IdType;
                Email = staff.Email;
                Phone = staff.Phone ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Basic Info Loaded");

                SelectedSkill = staff.Role;
                HourlyRate = staff.HourlyRate;
                SelectedEmploymentType = staff.EmploymentType;
                
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Setting Employment Date: {staff.EmploymentDate}");
                
                // Sanitize EmploymentDate
                if (staff.EmploymentDate < new DateTime(1900, 1, 1) || staff.EmploymentDate == DateTime.MinValue)
                {
                     System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] INVALID/MIN EmploymentDate detected: {staff.EmploymentDate}. Defaulting to Now.");
                     EmploymentDate = DateTimeOffset.Now;
                }
                else
                {
                    EmploymentDate = staff.EmploymentDate;
                }

                ContractDuration = staff.ContractDuration ?? string.Empty;
                Branch = staff.Branch;
                
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Setting Shift Times");
                ShiftStartTime = staff.ShiftStartTime;
                ShiftEndTime = staff.ShiftEndTime;

                // Leave Balances
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Setting Leave Balances");
                AnnualLeaveBalance = staff.AnnualLeaveBalance;
                SickLeaveBalance = staff.SickLeaveBalance;
                
                // Sanitize LeaveCycleStartDate
                if (staff.LeaveCycleStartDate.HasValue && 
                   (staff.LeaveCycleStartDate.Value < new DateTime(1900, 1, 1) || staff.LeaveCycleStartDate.Value == DateTime.MinValue))
                {
                     System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] INVALID/MIN LeaveCycleStartDate detected. Setting to null.");
                     LeaveCycleStartDate = null;
                }
                else
                {
                    LeaveCycleStartDate = staff.LeaveCycleStartDate.HasValue ? staff.LeaveCycleStartDate.Value : null;
                }

                // Set Initial Rule Text
                if (SelectedEmploymentType == EmploymentType.Contract)
                {
                     LeaveAccrualRule = "Accural: 1 day / 17 days (Annual) | 1 day / 26 days (Sick)";
                }
                else
                {
                     LeaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";
                }

                // Banking
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Setting Banking Details");
                AccountNumber = staff.AccountNumber ?? string.Empty;
                TaxNumber = staff.TaxNumber ?? string.Empty;
                BranchCode = staff.BranchCode ?? string.Empty;
                AccountType = string.IsNullOrEmpty(staff.AccountType) ? "Select Account Type" : staff.AccountType;
                SelectedRateType = staff.RateType;

                // Bank Selection Logic
                var dbBankName = staff.BankName;
                var matched = false;
                
                if (!string.IsNullOrEmpty(dbBankName))
                {
                    foreach (var bank in AvailableBanks)
                    {
                        // Skip None/Other during standard matching if desired, but here we just match description
                        if (bank == OCC.Shared.Models.BankName.None || bank == OCC.Shared.Models.BankName.Other) continue;

                        if (GetEnumDescription(bank).Equals(dbBankName, StringComparison.OrdinalIgnoreCase))
                        {
                            SelectedBank = bank;
                            matched = true;
                            break;
                        }
                    }
                }

                if (!matched && !string.IsNullOrEmpty(dbBankName))
                {
                    // Custom bank
                    SelectedBank = OCC.Shared.Models.BankName.Other;
                    CustomBankName = dbBankName;
                }
                else if (!matched)
                {
                    // Default to None (Placeholder)
                    SelectedBank = OCC.Shared.Models.BankName.None;
                    CustomBankName = string.Empty;
                }
                
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Triggering Property Changes");
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
                OnPropertyChanged(nameof(IsOtherBankSelected));
                
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Load Complete");
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] CRASH in Load: {ex.Message}");
                 System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Stack: {ex.StackTrace}");
                 throw;
            }
        }

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        #endregion

        #region Helper Methods

        partial void OnIdNumberChanged(string value)
        {
            if (SelectedIdType == IdType.RSAId && value.Length >= 6)
            {
                CalculateDoBFromRsaId(value);
            }
        }

        partial void OnSelectedIdTypeChanged(IdType value)
        {
             if (value == IdType.RSAId && IdNumber.Length >= 6)
            {
                CalculateDoBFromRsaId(IdNumber);
            }
        }

        partial void OnLeaveCycleStartDateChanged(DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                // SA BCEA: Sick leave cycle is 36 months (3 years) from start of employment or cycle
                var endDate = value.Value.AddMonths(36).AddDays(-1);
                SickLeaveCycleEndDisplay = endDate.ToString("dd MMM yyyy");
            }
            else
            {
                SickLeaveCycleEndDisplay = "N/A";
            }
        }

        [ObservableProperty]
        private BankName _selectedBank = OCC.Shared.Models.BankName.None;

        [ObservableProperty]
        private string _customBankName = string.Empty;

        public bool IsOtherBankSelected => SelectedBank == OCC.Shared.Models.BankName.Other;

        // Expose Bank Enum
        public OCC.Shared.Models.BankName[] AvailableBanks { get; } = Enum.GetValues<OCC.Shared.Models.BankName>();

        partial void OnSelectedBankChanged(BankName value)
        {
             OnPropertyChanged(nameof(IsOtherBankSelected));
        }

        partial void OnBranchChanged(string value)
        {
            // Default Times
            var jhbStart = new TimeSpan(7, 0, 0);
            var jhbEnd = new TimeSpan(16, 45, 0);
            var cptStart = new TimeSpan(7, 0, 0);
            var cptEnd = new TimeSpan(16, 30, 0);

            // Helper to check if time is a "known default" or null
            bool IsDefaultOrNull(TimeSpan? t) 
            {
                if (!t.HasValue) return true;
                return t.Value == jhbStart || t.Value == jhbEnd || t.Value == cptStart || t.Value == cptEnd;
            }

            // Only update if current times are standard defaults or null
            if (IsDefaultOrNull(ShiftStartTime) && IsDefaultOrNull(ShiftEndTime))
            {
                if (string.Equals(value, "Johannesburg", StringComparison.OrdinalIgnoreCase))
                {
                    ShiftStartTime = jhbStart;
                    ShiftEndTime = jhbEnd;
                }
                else if (string.Equals(value, "Cape Town", StringComparison.OrdinalIgnoreCase))
                {
                    ShiftStartTime = cptStart;
                    ShiftEndTime = cptEnd;
                }
            }
        }

        partial void OnFirstNameChanged(string value) => OnPropertyChanged(nameof(DisplayName));
        partial void OnLastNameChanged(string value) => OnPropertyChanged(nameof(DisplayName));

        partial void OnSelectedEmploymentTypeChanged(EmploymentType value)
        {
            if (value == EmploymentType.Contract)
            {
                LeaveAccrualRule = "Accural: 1 day / 17 days (Annual) | 1 day / 26 days (Sick)";
                // Suggest 0 defaults for new contract
                if (!_existingStaffId.HasValue) 
                {
                    AnnualLeaveBalance = 0;
                    SickLeaveBalance = 0;
                }
            }
            else
            {
                LeaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";
                 // Suggest defaults for new permanent if they were 0
                if (!_existingStaffId.HasValue && AnnualLeaveBalance == 0 && SickLeaveBalance == 0)
                {
                    AnnualLeaveBalance = 0; // Usually accumulate but start 0 too? Or pro-rata. Let's keep 0 safe or 1.25.
                    SickLeaveBalance = 30; // Standard cycle
                }
            }
        }

        private void CalculateDoBFromRsaId(string id)
        {
            if (id.Length < 6) return;
            string datePart = id.Substring(0, 6);
            
            if (DateTime.TryParseExact(datePart, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
            {
                _calculatedDoB = dob;
            }
        }

        #endregion
    }
}

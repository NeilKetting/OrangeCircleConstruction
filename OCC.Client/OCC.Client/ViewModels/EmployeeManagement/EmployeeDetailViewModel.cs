using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Employee> _staffRepository;
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
        private string _branchCode = string.Empty;

        [ObservableProperty]
        private string _accountType = "Select Account Type";

        public List<string> AccountTypes { get; } = new() { "Select Account Type", "Savings", "Cheque", "Transmission" };



        [ObservableProperty]
        private RateType _selectedRateType = RateType.Hourly;

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

        // ... existing properties ...

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

        public EmployeeDetailViewModel(IRepository<Employee> staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public EmployeeDetailViewModel() 
        {
            // _staffRepository will be null, handle in Save
            _staffRepository = null!;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Save()
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                return;

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
            staff.ShiftEndTime = ShiftEndTime;
            
            // Banking
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
            staff.BranchCode = BranchCode;
            
            // Map "Select Account Type" to null
            staff.AccountType = (AccountType == "Select Account Type") ? null : AccountType;
            
            staff.RateType = SelectedRateType;

            if (_staffRepository != null)
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

            _existingStaffId = staff.Id;
            Title = "Edit Employee";
            SaveButtonText = "Save Changes";

            EmployeeNumber = staff.EmployeeNumber;
            FirstName = staff.FirstName;
            LastName = staff.LastName;
            IdNumber = staff.IdNumber;
            SelectedIdType = staff.IdType;
            Email = staff.Email;
            Phone = staff.Phone ?? string.Empty;
            SelectedSkill = staff.Role;
            HourlyRate = staff.HourlyRate;
            SelectedEmploymentType = staff.EmploymentType;
            EmploymentDate = staff.EmploymentDate;
            ContractDuration = staff.ContractDuration ?? string.Empty;
            Branch = staff.Branch;
            ShiftStartTime = staff.ShiftStartTime;
            ShiftEndTime = staff.ShiftEndTime;

            // Banking
            AccountNumber = staff.AccountNumber ?? string.Empty;
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
            
            OnPropertyChanged(nameof(IsRsaId));
            OnPropertyChanged(nameof(IsPassport));
            OnPropertyChanged(nameof(IsHourly));
            OnPropertyChanged(nameof(IsSalary));
            OnPropertyChanged(nameof(IsPermanent));
            OnPropertyChanged(nameof(IsContract));
            OnPropertyChanged(nameof(IsContractVisible));
            OnPropertyChanged(nameof(IsOtherBankSelected));
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
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
        private DateTimeOffset _employmentDate = DateTimeOffset.Now;

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
        private TimeSpan? _shiftEndTime = new TimeSpan(17, 0, 0);

        #endregion

        #region Properties

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
            
            OnPropertyChanged(nameof(IsRsaId));
            OnPropertyChanged(nameof(IsPassport));
            OnPropertyChanged(nameof(IsPermanent));
            OnPropertyChanged(nameof(IsContract));
            OnPropertyChanged(nameof(IsContractVisible));
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

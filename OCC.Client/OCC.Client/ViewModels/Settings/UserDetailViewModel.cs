using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Settings
{
    public partial class UserDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<User> _userRepository;
        private Guid? _existingUserId;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? UserSaved;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty; // Only used for new users or resets

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private UserRole _selectedRole = UserRole.Guest;

        [ObservableProperty]
        private bool _isApproved;

        [ObservableProperty]
        private bool _isEmailVerified;

        [ObservableProperty]
        private string _title = "Add User";

        [ObservableProperty]
        private string _saveButtonText = "Add User";

        #endregion

        #region Properties

        public UserRole[] UserRoles { get; } = Enum.GetValues<UserRole>();

        public string DisplayName => $"{FirstName} {LastName}".Trim();

        #endregion

        #region Constructors

        public UserDetailViewModel(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public UserDetailViewModel()
        {
            // Designer constructor
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Email))
                return;

            User user;

            if (_existingUserId.HasValue)
            {
                // Update
                user = await _userRepository.GetByIdAsync(_existingUserId.Value) ?? new User { Id = _existingUserId.Value };
            }
            else
            {
                // Create
                user = new User();
                // For new users, set password if provided, else maybe default?
                // Real app would handle this better (hashing etc).
                if (!string.IsNullOrEmpty(Password))
                {
                    user.Password = Password; 
                }
            }

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Email = Email;
            user.Phone = Phone;
            user.Location = Location;
            user.UserRole = SelectedRole;
            user.IsApproved = IsApproved;
            user.IsEmailVerified = IsEmailVerified;

            if (_existingUserId.HasValue)
            {
                 // If updating and password field is not empty, update it
                if (!string.IsNullOrEmpty(Password))
                {
                    user.Password = Password;
                }
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                await _userRepository.AddAsync(user);
            }

            UserSaved?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public void Load(User user)
        {
            if (user == null) return;

            _existingUserId = user.Id;
            Title = "Edit User";
            SaveButtonText = "Save Changes";

            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Phone = user.Phone ?? string.Empty;
            Location = user.Location ?? string.Empty;
            SelectedRole = user.UserRole;
            IsApproved = user.IsApproved;
            IsEmailVerified = user.IsEmailVerified;
            
            // We typically don't load the password back into the UI
            Password = "";

            OnPropertyChanged(nameof(DisplayName));
        }

        partial void OnFirstNameChanged(string value) => OnPropertyChanged(nameof(DisplayName));
        partial void OnLastNameChanged(string value) => OnPropertyChanged(nameof(DisplayName));

        #endregion
    }
}

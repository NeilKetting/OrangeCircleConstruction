using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Settings
{
    public partial class UserManagementViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<User> _userRepository;
        private List<User> _allUsers = new();

        #endregion

        #region Observables

        [ObservableProperty]
        private string _activeFilter = "All";

        [ObservableProperty]
        private int _totalUsers = 0;

        [ObservableProperty]
        private int _pendingApprovalCount = 0;

        [ObservableProperty]
        private int _adminCount = 0;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private bool _isUserPopupVisible;

        [ObservableProperty]
        private UserDetailViewModel? _userPopup;

        [ObservableProperty]
        private User? _selectedUser;

        #endregion

        #region Constructors

        public UserManagementViewModel()
        {
            // Designer support
        }

        public UserManagementViewModel(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
            LoadData();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddUser()
        {
            UserPopup = new UserDetailViewModel(_userRepository);
            UserPopup.CloseRequested += (s, e) => IsUserPopupVisible = false;
            UserPopup.UserSaved += (s, e) => 
            {
                IsUserPopupVisible = false;
                LoadData();
            };
            IsUserPopupVisible = true;
        }

        [RelayCommand]
        public void EditUser(User user)
        {
            if (user == null) return;

            UserPopup = new UserDetailViewModel(_userRepository);
            UserPopup.Load(user);
            UserPopup.CloseRequested += (s, e) => IsUserPopupVisible = false;
            UserPopup.UserSaved += (s, e) => 
            {
                IsUserPopupVisible = false;
                LoadData(); 
            };
            IsUserPopupVisible = true;
        }

        [RelayCommand]
        public async Task DeleteUser(User user)
        {
            if (user == null) return;
            await _userRepository.DeleteAsync(user.Id);
            LoadData();
        }

        [RelayCommand]
        public async Task ApproveUser(User user)
        {
            if (user == null) return;
            user.IsApproved = true;
            await _userRepository.UpdateAsync(user);
            LoadData(); // Refresh counts
        }

        [RelayCommand]
        private void SetFilter(string filter)
        {
            ActiveFilter = filter;
            FilterUsers();
        }

        #endregion

        #region Methods

        public async void LoadData()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                _allUsers = users.ToList();
                
                TotalUsers = _allUsers.Count;
                PendingApprovalCount = _allUsers.Count(u => !u.IsApproved);
                AdminCount = _allUsers.Count(u => u.UserRole == UserRole.Admin);

                FilterUsers();
                OnPropertyChanged(nameof(TotalUsers));
                OnPropertyChanged(nameof(PendingApprovalCount));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex}");
            }
        }

        partial void OnSearchQueryChanged(string value) => FilterUsers();

        private void FilterUsers()
        {
            var filtered = _allUsers.AsEnumerable();

            // Text search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.ToLower();
                filtered = filtered.Where(u => 
                    u.FirstName.ToLower().Contains(q) || 
                    u.LastName.ToLower().Contains(q) || 
                    u.Email.ToLower().Contains(q));
            }

            // Category filter
            switch (ActiveFilter)
            {
                case "Pending":
                    filtered = filtered.Where(u => !u.IsApproved);
                    break;
                case "Admins":
                    filtered = filtered.Where(u => u.UserRole == UserRole.Admin);
                    break;
                case "All":
                default:
                    break;
            }

            Users = new ObservableCollection<User>(filtered);
        }

        #endregion
    }
}

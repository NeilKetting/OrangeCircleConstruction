using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using OCC.Client.Services.Infrastructure; // Fixed
using OCC.Client.ViewModels.Core; // Fixed
using OCC.Client.ViewModels.Messages; // Fixed
using System.Collections.Generic;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class TeamDetailViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<EntityUpdatedMessage>
    {
        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<TeamMember> _teamMemberRepository;
        private readonly IRepository<Employee> _employeeRepository;

        private Guid? _existingTeamId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _title = "Add Team";

        public ObservableCollection<TeamMemberDisplay> Members { get; } = new();
        
        // Setup for adding members
        public ObservableCollection<Employee> AvailableEmployees { get; } = new();
        
        [ObservableProperty]
        private Employee? _selectedEmployeeToAdd;

        public event EventHandler? CloseRequested;
        public event EventHandler? TeamSaved;

        public TeamDetailViewModel(
            IRepository<Team> teamRepository, 
            IRepository<TeamMember> teamMemberRepository,
            IRepository<Employee> employeeRepository)
        {
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _employeeRepository = employeeRepository;
            
            CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.RegisterAll(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default, this);
        }

        public void Load(Team team)
        {
            if (team.Id != Guid.Empty && !string.IsNullOrEmpty(team.Name))
            {
                _existingTeamId = team.Id;
                Name = team.Name;
                Description = team.Description;
                Title = "Edit Team";
                LoadMembers();
            }
            else
            {
                _existingTeamId = null;
                Name = "";
                Description = "";
                Title = "Add Team";
                Members.Clear();
            }
            LoadEmployees();
        }

        private async void LoadMembers()
        {
            if (!_existingTeamId.HasValue) return;

            // Fetch members
            var allMembers = await _teamMemberRepository.GetAllAsync();
            var teamMembers = allMembers.Where(tm => tm.TeamId == _existingTeamId.Value).ToList();
            
            var allEmployees = await _employeeRepository.GetAllAsync(); // Cache this?

            Members.Clear();
            foreach(var tm in teamMembers)
            {
                var emp = allEmployees.FirstOrDefault(e => e.Id == tm.EmployeeId);
                if (emp != null)
                {
                    Members.Add(new TeamMemberDisplay { 
                        Id = tm.Id, 
                        Name = $"{emp.FirstName} {emp.LastName}", 
                        Role = emp.Role.ToString(),
                        EmployeeId = emp.Id
                    });
                }
            }
        }

        private async void LoadEmployees()
        {
            var emps = await _employeeRepository.GetAllAsync();
            AvailableEmployees.Clear();
            foreach(var e in emps) AvailableEmployees.Add(e);
        }

        public void Receive(EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "TeamMember" || (message.Value.EntityType == "Team" && message.Value.Id == _existingTeamId))
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(LoadMembers);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            Team team = new Team();
            if (_existingTeamId.HasValue)
            {
                team.Id = _existingTeamId.Value;
                // If we fetch to keep other props? 
            }
            
            team.Name = Name;
            team.Description = Description;
            
            if (_existingTeamId.HasValue)
                await _teamRepository.UpdateAsync(team);
            else
                await _teamRepository.AddAsync(team);

            TeamSaved?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        
        [RelayCommand]
        private async Task AddMember()
        {
            if (SelectedEmployeeToAdd == null || !_existingTeamId.HasValue) return;
            
            // Check if already exists
            if (Members.Any(m => m.EmployeeId == SelectedEmployeeToAdd.Id)) return;

            var newMember = new TeamMember
            {
                TeamId = _existingTeamId.Value,
                EmployeeId = SelectedEmployeeToAdd.Id
            };
            
            await _teamMemberRepository.AddAsync(newMember);
            // SignalR update will refresh list
            SelectedEmployeeToAdd = null; 
        }

        [RelayCommand]
        private async Task RemoveMember(TeamMemberDisplay member)
        {
            if (member == null) return;
            await _teamMemberRepository.DeleteAsync(member.Id);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class TeamMemberDisplay
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public Guid EmployeeId { get; set; }
    }
}

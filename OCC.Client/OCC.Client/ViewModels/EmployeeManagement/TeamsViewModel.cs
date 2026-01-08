using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class TeamsViewModel : ViewModelBase
    {
        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<Employee> _employeeRepository; // Need to check for duplicates if implementing logic?

        [ObservableProperty]
        private ObservableCollection<Team> _teams = new();

        [ObservableProperty]
        private Team? _selectedTeam;

        [ObservableProperty]
        private bool _isBusy;

        public TeamsViewModel(IRepository<Team> teamRepository, IRepository<Employee> employeeRepository)
        {
            _teamRepository = teamRepository;
            _employeeRepository = employeeRepository;
            _ = LoadTeams();
        }

        [RelayCommand]
        private async Task LoadTeams()
        {
            IsBusy = true;
            try
            {
                var teams = await _teamRepository.GetAllAsync();
                Teams = new ObservableCollection<Team>(teams);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateTeam()
        {
            // Simple create for now, user can edit details in a popup/detail view (to be implemented)
            var newTeam = new Team { Name = "New Team", Description = "Team Description" };
            await _teamRepository.AddAsync(newTeam);
            Teams.Add(newTeam);
            SelectedTeam = newTeam;
            // TODO: Open Edit Dialog or logic to add members
        }
        
        [RelayCommand]
        private async Task DeleteTeam(Team team)
        {
            if (team == null) return;
            await _teamRepository.DeleteAsync(team.Id);
            Teams.Remove(team);
        }
    }
}

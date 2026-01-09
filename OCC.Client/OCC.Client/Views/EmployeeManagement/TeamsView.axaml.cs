using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class TeamsView : UserControl
    {
        public TeamsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
             if (DataContext is ViewModels.EmployeeManagement.TeamManagementViewModel vm && 
                sender is Avalonia.Controls.DataGrid grid && 
                grid.SelectedItem is OCC.Shared.Models.Team team)
            {
                vm.EditTeamCommand.Execute(team);
            }
        }
    }
}

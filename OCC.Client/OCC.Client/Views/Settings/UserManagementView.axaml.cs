using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Settings
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
             if (DataContext is ViewModels.Settings.UserManagementViewModel vm && 
                sender is Avalonia.Controls.DataGrid grid && 
                grid.SelectedItem is OCC.Shared.Models.User user)
            {
                vm.EditUserCommand.Execute(user);
            }
        }
    }
}

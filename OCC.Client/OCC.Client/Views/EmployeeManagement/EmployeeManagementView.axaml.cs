using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.EmployeeManagement;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class EmployeeManagementView : UserControl
    {
        public EmployeeManagementView()
        {
            InitializeComponent();
        }

        private void DataGrid_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is EmployeeManagementViewModel vm && vm.SelectedEmployee != null)
            {
                vm.EditEmployeeCommand.Execute(vm.SelectedEmployee);
            }
        }
    }
}

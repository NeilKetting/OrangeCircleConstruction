using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.Shared;

namespace OCC.Client.Views.Shared
{
    public partial class SidebarView : UserControl
    {
        public SidebarView()
        {
            InitializeComponent();
        }

        private void Sidebar_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.Focus();
        }

        private void Sidebar_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is SidebarViewModel vm)
            {
                vm.ToggleCollapse();
            }
        }
    }
}

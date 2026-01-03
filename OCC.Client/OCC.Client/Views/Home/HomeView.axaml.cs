using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.Home;

namespace OCC.Client.Views.Home
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source is Grid grid && grid.Name == "OverlayGrid")
            {
                if (DataContext is HomeViewModel vm)
                {
                    vm.CloseTaskDetailCommand.Execute(null);
                }
            }
        }
    }
}

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class OrderDashboardView : UserControl
    {
        public OrderDashboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

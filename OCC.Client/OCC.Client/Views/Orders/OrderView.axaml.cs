using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

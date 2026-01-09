using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class CreateOrderView : UserControl
    {
        public CreateOrderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

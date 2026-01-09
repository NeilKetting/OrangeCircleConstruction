using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

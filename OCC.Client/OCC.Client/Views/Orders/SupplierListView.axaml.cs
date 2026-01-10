using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Orders
{
    public partial class SupplierListView : UserControl
    {
        public SupplierListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

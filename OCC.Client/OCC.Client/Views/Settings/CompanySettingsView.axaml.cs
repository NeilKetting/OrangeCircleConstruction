using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Settings
{
    public partial class CompanySettingsView : UserControl
    {
        public CompanySettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

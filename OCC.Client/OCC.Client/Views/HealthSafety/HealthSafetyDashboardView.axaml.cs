using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.HealthSafety
{
    public partial class HealthSafetyDashboardView : UserControl
    {
        public HealthSafetyDashboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

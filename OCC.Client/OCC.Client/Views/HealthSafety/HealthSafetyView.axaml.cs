using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.HealthSafety
{
    public partial class HealthSafetyView : UserControl
    {
        public HealthSafetyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

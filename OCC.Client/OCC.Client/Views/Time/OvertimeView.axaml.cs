using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Time
{
    public partial class OvertimeView : UserControl
    {
        public OvertimeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

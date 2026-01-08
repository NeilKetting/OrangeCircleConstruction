using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Time
{
    public partial class TimeMenuView : UserControl
    {
        public TimeMenuView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Time
{
    public partial class TimeLiveView : UserControl
    {
        public TimeLiveView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

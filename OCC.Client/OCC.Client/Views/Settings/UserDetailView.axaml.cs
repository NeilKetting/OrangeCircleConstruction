using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Settings
{
    public partial class UserDetailView : UserControl
    {
        public UserDetailView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class TeamsView : UserControl
    {
        public TeamsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

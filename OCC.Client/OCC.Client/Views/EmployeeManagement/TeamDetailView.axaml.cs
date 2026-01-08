using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class TeamDetailView : UserControl
    {
        public TeamDetailView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

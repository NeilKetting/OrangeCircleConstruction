using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Home.ProjectSummary
{
    public partial class ProjectSummaryView : UserControl
    {
        public ProjectSummaryView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

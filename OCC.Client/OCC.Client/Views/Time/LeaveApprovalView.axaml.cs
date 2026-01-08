using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OCC.Client.Views.Time
{
    public partial class LeaveApprovalView : UserControl
    {
        public LeaveApprovalView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

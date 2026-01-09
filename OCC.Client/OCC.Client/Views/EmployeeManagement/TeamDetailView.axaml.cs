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

        public void FocusInput()
        {
             var input = this.FindControl<TextBox>("NameInput");
             input?.Focus();
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

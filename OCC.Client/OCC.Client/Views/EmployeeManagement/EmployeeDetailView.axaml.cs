using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class EmployeeDetailView : UserControl
    {
        public EmployeeDetailView()
        {
            InitializeComponent();
        }



        public void FocusInput()
        {
             var input = this.FindControl<TextBox>("EmployeeNumberInput");
             input?.Focus();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsVisibleProperty && change.NewValue is true)
            {
                var input = this.FindControl<TextBox>("EmployeeNumberInput");
                if (input != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => input.Focus());
                }
            }
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

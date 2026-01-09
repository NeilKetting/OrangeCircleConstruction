using Avalonia.Controls;
using Avalonia.Interactivity;
using OCC.Client.ViewModels.Home.Tasks;

namespace OCC.Client.Views.Home.List.Widgets
{
    public partial class TaskDetailView : UserControl
    {
        public TaskDetailView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // Self-focus on attach is good, but explicit FocusInput called by parent is more robust for visibility toggles
        }

        public void FocusInput()
        {
            this.Focus();
        }

        private void Duration_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is TaskDetailViewModel vm)
            {
                vm.CommitDurationsCommand.Execute(null);
            }
        }
    }
}

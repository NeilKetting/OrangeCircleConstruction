using Avalonia.Controls;
using Avalonia.Interactivity;
using OCC.Client.ViewModels.Home.Tasks;

namespace OCC.Client.Views.Home.Tasks
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

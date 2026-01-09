using Avalonia.Controls;
using Avalonia.Input;
using OCC.Client.ViewModels.EmployeeManagement;
using System;
using Avalonia;

namespace OCC.Client.Views.EmployeeManagement
{
    public partial class EmployeeManagementView : UserControl
    {
        public EmployeeManagementView()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is EmployeeManagementViewModel vm)
                {
                    if (vm.IsAddEmployeePopupVisible)
                    {
                        vm.IsAddEmployeePopupVisible = false;
                        e.Handled = true;
                        return;
                    }
                    if (vm.IsAddTeamPopupVisible)
                    {
                        vm.IsAddTeamPopupVisible = false;
                        e.Handled = true;
                        return;
                    }
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // Keep focus logic as it's still good UX for typing, even if not needed for Esc
            var detailView = this.FindControl<EmployeeDetailView>("DetailView");
            if (detailView != null)
            {
                detailView.PropertyChanged += async (s, args) =>
                {
                    if (args.Property.Name == nameof(Visual.IsEffectivelyVisible) && args.NewValue is true)
                    {
                        // Slight delay to ensure visual tree is ready
                        await System.Threading.Tasks.Task.Delay(50);
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => detailView.FocusInput(), Avalonia.Threading.DispatcherPriority.Background);
                    }
                };
            }

            var teamDetailView = this.FindControl<TeamDetailView>("TeamDetailView");
            if (teamDetailView != null)
            {
                teamDetailView.PropertyChanged += async (s, args) =>
                {
                    if (args.Property.Name == nameof(Visual.IsEffectivelyVisible) && args.NewValue is true)
                    {
                        await System.Threading.Tasks.Task.Delay(50);
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => teamDetailView.FocusInput(), Avalonia.Threading.DispatcherPriority.Background);
                    }
                };
            }
        }
    }
}

using Avalonia.Controls;
using System;
using System.Globalization;

namespace OCC.Client.Views.Home.Calendar
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
        }

        private void OnTaskPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender is Border border && border.DataContext is ViewModels.Home.Calendar.CalendarTaskViewModel task)
            {
                var popup = this.FindControl<Border>("TaskHoverPopup");
                var title = this.FindControl<TextBlock>("PopupTitle");
                var time = this.FindControl<TextBlock>("PopupTime");
                var status = this.FindControl<TextBlock>("PopupStatus");

                if (popup != null && title != null && time != null && status != null)
                {
                    title.Text = task.Name;
                    
                    // Format Date Range
                    string dateRange = task.Start.Day == task.End.Day 
                        ? task.Start.ToString("MMM dd") 
                        : $"{task.Start:MMM dd} - {task.End:MMM dd}";
                    
                    time.Text = dateRange;
                    status.Text = task.IsCompleted ? "Completed" : "In Progress";
                    
                    popup.IsVisible = true;
                    
                    var position = e.GetPosition(this);
                    // Offset slightly so cursor doesn't cover text
                    popup.RenderTransform = new Avalonia.Media.TranslateTransform(position.X + 15, position.Y + 15);
                }
            }
        }

        private void OnTaskPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var popup = this.FindControl<Border>("TaskHoverPopup");
            if (popup != null)
            {
                popup.IsVisible = false;
            }
        }
    }

}

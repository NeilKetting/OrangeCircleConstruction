using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ShowBugReportAsync(string viewName)
        {
             if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
             {
                 var dialog = new OCC.Client.Views.Bugs.BugReportDialog();
                 var vm = new OCC.Client.ViewModels.Bugs.BugReportDialogViewModel(
                     Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBugReportService>(_serviceProvider),
                     Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IAuthService>(_serviceProvider),
                     viewName,
                     () => Avalonia.Threading.Dispatcher.UIThread.Post(() => dialog.Close())
                 );
                 dialog.DataContext = vm;
                 await dialog.ShowDialog(desktop.MainWindow);
             }
        }

        public async Task<string?> PickFileAsync(string title, IEnumerable<string> extensions)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel == null) return null;

                var options = new FilePickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Supported Files")
                        {
                             Patterns = extensions?.ToList() 
                        }
                    }
                };

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
                return files.FirstOrDefault()?.Path.LocalPath;
            }
            return null;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var dialog = new OCC.Client.Views.ConfirmationDialog(title, message);
                await dialog.ShowDialog(desktop.MainWindow);
                return dialog.Result;
            }
            return false;
        }

        public async Task ShowAlertAsync(string title, string message)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                // Re-use ConfirmationDialog but hide Cancel button? Or just use it as is for now.
                // Minimal implementation:
                var dialog = new OCC.Client.Views.ConfirmationDialog(title, message);
                // We'd ideally hide the Cancel button, but for MVP it's fine.
                await dialog.ShowDialog(desktop.MainWindow);
            }
        }
        public async Task<(bool Confirmed, string? Reason, string? Note)> ShowLeaveEarlyReasonAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var dialog = new OCC.Client.Views.Time.LeaveEarlyReasonDialog();
                var result = await dialog.ShowDialog<bool?>(desktop.MainWindow);
                
                if (result == true)
                {
                    return (true, dialog.Reason, dialog.Note);
                }
            }
            return (false, null, null);
        }
        public async Task<(bool Confirmed, TimeSpan? InTime, TimeSpan? OutTime)> ShowEditAttendanceAsync(TimeSpan? currentIn, TimeSpan? currentOut)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var dialog = new OCC.Client.Views.Time.EditAttendanceDialog(currentIn, currentOut);
                var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
                
                if (result)
                {
                    return (true, dialog.ClockInTime, dialog.ClockOutTime);
                }
            }
            return (false, null, null);
        }
        public async Task<bool> ShowSessionTimeoutAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var dialog = new OCC.Client.Views.Confirmation.SessionTimeoutDialog();
                // Show as Modal?
                // Depending on requirements, maybe ShowDialog is best to block input.
                var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
                return result;
            }
            return false;
        }
    }
}

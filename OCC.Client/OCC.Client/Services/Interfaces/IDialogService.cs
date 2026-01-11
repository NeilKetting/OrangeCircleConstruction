using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IDialogService
    {
        Task<string?> PickFileAsync(string title, IEnumerable<string> extensions);
        Task ShowBugReportAsync(string viewName);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowAlertAsync(string title, string message);
        Task<(bool Confirmed, string? Reason, string? Note)> ShowLeaveEarlyReasonAsync();
        Task<(bool Confirmed, TimeSpan? InTime, TimeSpan? OutTime)> ShowEditAttendanceAsync(TimeSpan? currentIn, TimeSpan? currentOut);
        Task<bool> ShowSessionTimeoutAsync();
    }
}

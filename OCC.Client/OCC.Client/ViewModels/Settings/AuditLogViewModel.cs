using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using System;

namespace OCC.Client.ViewModels.Settings
{
    public partial class AuditLogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<AuditLog> _logs = new();

        public AuditLogViewModel()
        {
            // DESIGN-TIME MOCK DATA
            Logs.Add(new AuditLog { Action = "Login", UserId = "System", Timestamp = DateTime.Now, NewValues = "User Logged In" });
            Logs.Add(new AuditLog { Action = "Update", TableName = "Employee", RecordId = "101", UserId = "Admin", Timestamp = DateTime.Now.AddMinutes(-5), OldValues = "{ 'Name': 'John' }", NewValues = "{ 'Name': 'Johnny' }" });
        }
    }
}

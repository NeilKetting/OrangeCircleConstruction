using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public interface ITimeService
    {
        Task<IEnumerable<TimeRecord>> GetWeeklyTimeRecordsAsync(DateTime weekStart);
        Task SaveTimeRecordAsync(TimeRecord record);
        Task<IEnumerable<AttendanceRecord>> GetDailyAttendanceAsync(DateTime date);
        Task SaveAttendanceRecordAsync(AttendanceRecord record);
        Task<IEnumerable<StaffMember>> GetAllStaffAsync();
    }
}

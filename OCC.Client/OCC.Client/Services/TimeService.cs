using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services
{
    public class TimeService : ITimeService
    {
        private readonly IRepository<TimeRecord> _timeRepository;
        private readonly IRepository<AttendanceRecord> _attendanceRepository;
        private readonly IRepository<Employee> _staffRepository;

        public TimeService(
            IRepository<TimeRecord> timeRepository,
            IRepository<AttendanceRecord> attendanceRepository,
            IRepository<Employee> staffRepository)
        {
            _timeRepository = timeRepository;
            _attendanceRepository = attendanceRepository;
            _staffRepository = staffRepository;
        }

        public async Task<IEnumerable<TimeRecord>> GetWeeklyTimeRecordsAsync(DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            return await _timeRepository.FindAsync(r => r.Date >= weekStart && r.Date < weekEnd);
        }

        public async Task SaveTimeRecordAsync(TimeRecord record)
        {
            if (record.Id == Guid.Empty || (await _timeRepository.GetByIdAsync(record.Id)) == null)
            {
                await _timeRepository.AddAsync(record);
            }
            else
            {
                await _timeRepository.UpdateAsync(record);
            }
        }

        public async Task<IEnumerable<AttendanceRecord>> GetDailyAttendanceAsync(DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            return await _attendanceRepository.FindAsync(r => r.Date >= dayStart && r.Date < dayEnd);
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceByRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure end date is inclusive of the day (e.g., if user picks 2026-01-08, we want up to 2026-01-08 23:59:59)
            // If the incoming View Logic gives "Today" as start:Today, end:Today, we want full day.
            // Usually best to treat EndDate as "End of Day" or next day 00:00.
            // But let's assume the VM gives strict boundaries, or we standardize here.
            
            // Standardizing: Date portion comparison or simple range.
            // Best practice: >= Start AND < End (where End is +1 day of the visualization range).
            
            return await _attendanceRepository.FindAsync(r => r.Date >= startDate && r.Date <= endDate);
        }

        public async Task SaveAttendanceRecordAsync(AttendanceRecord record)
        {
            if (record.Id == Guid.Empty || (await _attendanceRepository.GetByIdAsync(record.Id)) == null)
            {
                await _attendanceRepository.AddAsync(record);
            }
            else
            {
                await _attendanceRepository.UpdateAsync(record);
            }
        }

        public async Task ClearAllAttendanceAsync()
        {
            try
            {
                // For development: fetch all and delete.
                var allRecords = await _attendanceRepository.GetAllAsync();
                foreach (var record in allRecords)
                {
                    await _attendanceRepository.DeleteAsync(record.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing attendance: {ex.Message}");
                // In production, log properly
            }
        }

        public async Task<IEnumerable<Employee>> GetAllStaffAsync()
        {
            return await _staffRepository.GetAllAsync();
        }
    }
}

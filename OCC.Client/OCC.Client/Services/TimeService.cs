using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class TimeService : ITimeService
    {
        private readonly IRepository<TimeRecord> _timeRepository;
        private readonly IRepository<AttendanceRecord> _attendanceRepository;
        private readonly IRepository<StaffMember> _staffRepository;

        public TimeService(
            IRepository<TimeRecord> timeRepository,
            IRepository<AttendanceRecord> attendanceRepository,
            IRepository<StaffMember> staffRepository)
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

        public async Task<IEnumerable<StaffMember>> GetAllStaffAsync()
        {
            return await _staffRepository.GetAllAsync();
        }
    }
}

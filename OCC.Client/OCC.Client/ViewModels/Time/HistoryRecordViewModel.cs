using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;

namespace OCC.Client.ViewModels.Time
{
    public partial class HistoryRecordViewModel : ObservableObject
    {
        private readonly AttendanceRecord _attendance;
        private readonly Employee _employee;

        public HistoryRecordViewModel(AttendanceRecord attendance, Employee employee)
        {
            _attendance = attendance;
            _employee = employee;
        }

        public DateTime Date => _attendance.Date;
        public string EmployeeName => _employee.DisplayName;
        public string Branch => _attendance.Branch; // Or employee branch? Attendance branch preserves history.
        
        // Times
        public string InTime => _attendance.CheckInTime?.ToString("HH:mm") ?? _attendance.ClockInTime?.ToString(@"hh\:mm") ?? "--:--";
        public string OutTime => _attendance.CheckOutTime?.ToString("HH:mm") ?? "--:--";
        public string Status => _attendance.Status.ToString();

        // Calculations
        public double HoursWorked
        {
            get
            {
                var checkOut = _attendance.CheckOutTime;
                
                // Live calculation for active sessions (regardless of start date)
                if (!checkOut.HasValue)
                {
                    checkOut = DateTime.Now;
                }

                if (_attendance.CheckInTime.HasValue)
                {
                    if (checkOut.HasValue)
                        return (checkOut.Value - _attendance.CheckInTime.Value).TotalHours;
                }
                
                // Fallback for manual ClockInTime + CheckOutTime/Now
                if (_attendance.ClockInTime.HasValue)
                {
                    var inDt = _attendance.Date.Add(_attendance.ClockInTime.Value);
                    if (checkOut.HasValue)
                        return (checkOut.Value - inDt).TotalHours;
                }
                return 0;
            }
        }
        
        public string HoursWorkedDisplay => HoursWorked > 0 ? $"{HoursWorked:F2}" : "-";

        public decimal Wage
        {
            get
            {
                if (_employee.RateType == RateType.Hourly)
                {
                    // return (decimal)(HoursWorked * _employee.HourlyRate);
                    // NEW: Detailed Calculation (Bucket Logic)
                    return CalculateAccurateWage();
                }
                // RateType.MonthlySalary
                return 0;
            }
        }
        
        private decimal CalculateAccurateWage()
        {
             // 1. Get Start and End Times
             DateTime start;
             if (_attendance.CheckInTime.HasValue) start = _attendance.CheckInTime.Value;
             else if (_attendance.ClockInTime.HasValue) start = _attendance.Date.Add(_attendance.ClockInTime.Value);
             else return 0;

             DateTime end;
             if (_attendance.CheckOutTime.HasValue) end = _attendance.CheckOutTime.Value;
             else end = DateTime.Now; // Live calculation currently

             if (start >= end) return 0;

             decimal totalWage = 0;
             // Fix: Cast double to decimal for the fallback so ?? works
             decimal rateToUse = _attendance.CachedHourlyRate ?? (decimal)_employee.HourlyRate;
             double hourlyRate = (double)rateToUse;
             string branch = _attendance.Branch ?? _employee.Branch ?? "Johannesburg";
             
             // 2. Iterate through time in small chunks (e.g. 15 mins) or analyze spans.
             // For precision and handling "cross-midnight" easily, let's step through.
             // Optimization: Step by 30 mins or calculate span intersections.
             // Given the requirements, a span intersect approach is better but complex to write inline.
             // Let's use a "Chunking" loop (15 min intervals).
             
             var current = start;
             var interval = TimeSpan.FromMinutes(15);
             
             while (current < end)
             {
                 var next = current.Add(interval);
                 if (next > end) next = end;
                 
                 var durationHours = (next - current).TotalHours;
                 var multiplier = GetMultiplier(current, branch);
                 
                 totalWage += (decimal)(durationHours * hourlyRate * multiplier);
                 
                 current = next;
             }
             
             return totalWage;
        }

        private double GetMultiplier(DateTime time, string branch)
        {
            // 1. Sundays = 2.0x
            if (time.DayOfWeek == DayOfWeek.Sunday) return 2.0;

            // 2. Saturdays = 1.5x
            if (time.DayOfWeek == DayOfWeek.Saturday) return 1.5;

            // 3. Public Holidays (TODO: Inject Holiday Service or hardcode known list for now)
            // if (IsHoliday(time)) return 2.0;

            // 4. Weekday Overtime (After 16:00 JHB / 17:00 CPT) = 1.5x
            int endHour = branch.Contains("Cape", StringComparison.OrdinalIgnoreCase) ? 17 : 16;
            
            // If before 07:00 start? Usually early starts are also OT, but focused on late for now.
            // Requirement was: "Weekdays after normal hours". 
            // Normal hours usually end at 16:00/17:00.
            if (time.Hour >= endHour) return 1.5;
            
            // Early morning? (Before 7/8). Assuming standard day matches Overtime logic.
            // For now, Standard Rate.
            return 1.0;
        }
        
        public string WageDisplay => _employee.RateType == RateType.Hourly ? $"{Wage:C}" : "Salary";

        public void Refresh()
        {
            OnPropertyChanged(nameof(HoursWorked));
            OnPropertyChanged(nameof(HoursWorkedDisplay));
            OnPropertyChanged(nameof(Wage));
            OnPropertyChanged(nameof(WageDisplay));
        }

        // Expose underlying data for Export
        public Employee Employee => _employee;
        public AttendanceRecord Attendance => _attendance;
    }
}

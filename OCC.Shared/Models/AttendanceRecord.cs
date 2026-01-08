using System;

namespace OCC.Shared.Models
{
    public class AttendanceRecord : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Links to either a system User or a StaffMember
        public Guid? UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        
        // Geolocation for mobile check-ins
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        
        public string? Notes { get; set; }
        public string? LeaveReason { get; set; }
        public string? DoctorsNoteImagePath { get; set; }

        public string Branch { get; set; } = string.Empty;
        public TimeSpan? ClockInTime { get; set; } // Default 07:00 logic in VM
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Sick,
        LeaveEarly,
        LeaveAuthorized
    }
}

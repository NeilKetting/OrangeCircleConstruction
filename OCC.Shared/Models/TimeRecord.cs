using System;

namespace OCC.Shared.Models
{
    public class TimeRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid? UserId { get; set; }
        public Guid? StaffId { get; set; }
        
        public Guid ProjectId { get; set; }
        public Guid TaskId { get; set; }
        
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        
        public string? Comment { get; set; }        
    }
}

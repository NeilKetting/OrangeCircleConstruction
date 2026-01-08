using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCC.Shared.Models
{
    public class OvertimeRequest : IEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; } // The day the overtime is for

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending; // Reuse LeaveStatus (Pending, Approved, Rejected) or make new Status Enum? Reuse is fine for now/Simple.

        public Guid? ApproverId { get; set; }
        public DateTime? ActionedDate { get; set; }
        public string? AdminComment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Calculated duration in hours.
        /// </summary>
        [NotMapped]
        public double DurationHours => (EndTime - StartTime).TotalHours;
    }
}
